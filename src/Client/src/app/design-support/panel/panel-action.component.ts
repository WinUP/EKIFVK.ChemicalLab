import { Component, Input, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { MessageService } from '../../server/message.service';
import { StorageService } from '../../server/storage.service';
import { SynchronizedEventService } from '../../server/synchronized-event.service'
import { Messages, Events, EventArgs, StorageType, CacheStorageKey } from '../../server/structure';

@Component({
    selector: 'design-panel-action',
    templateUrl: './panel-action.component.html',
    styleUrls: ['./style.scss']
})
export class PanelActionComponent implements OnInit, AfterViewInit, OnDestroy {
    @Input() public title: string;
    @Input() public initialize: (action: PanelActionComponent) => void;
    @Input() public action: string = null;
    private messageListener: Subscription;
    private active: boolean = false;

    constructor(public message: MessageService, public storage: StorageService, public event: SynchronizedEventService) { }

    public ngOnInit(): void {
        if (this.initialize != null && this.initialize != undefined)
            this.initialize(this);
    }

    public ngAfterViewInit(): void {
        this.messageListener = this.message.listen(m => {
            if (this.action != null && m.is(Messages.CardActionClick))
                this.active = m.read<string>() == this.action;
        });
    }

    public ngOnDestroy(): void {
        this.messageListener.unsubscribe();
    }

    public toggleActive(): void {
        this.active = !this.active;
    }

    public onClick(event): void {
        var result: EventArgs = { canceled: false };
        if (this.event.fire(Events.CardActionClick, result).canceled) return;
        this.storage.cache(CacheStorageKey.PreviousCardAction,
            this.storage.read(StorageType.Cache, CacheStorageKey.CurrentCardAction));
        this.storage.cache(CacheStorageKey.CurrentCardAction, this.action);
        this.message.prepare().tag(Messages.CardActionClick).value(this.action).go();
    }
}
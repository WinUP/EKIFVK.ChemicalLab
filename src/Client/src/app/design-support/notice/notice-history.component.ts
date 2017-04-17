import { Component, AfterViewInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { MessageService } from '../../server/message.service';
import { Messages } from '../../server/structure'
import { Notice } from './notice';

@Component({
    selector: 'design-notice-history',
    templateUrl: './notice-history.component.html',
    styleUrls: ['./style.scss']
})
export class NoticeHistoryComponent implements AfterViewInit, OnDestroy {
    private notices: Array<Notice> = new Array<Notice>();
    private messageListener: Subscription;

    constructor(public message: MessageService) { }

    public ngAfterViewInit(): void {
        this.messageListener = this.message.listen(m => {
            if (m.is(Messages.PushNoticeHistory))
                this.notices.splice(0, 0, m.read<Notice>());
        });
    }

    public ngOnDestroy(): void {
        this.messageListener.unsubscribe();
    }

    public remove(target: Notice): void {
        var index = this.notices.indexOf(target);
        if (index > -1) this.notices.splice(index, 1);
    }
}
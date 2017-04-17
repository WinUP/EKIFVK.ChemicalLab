import { Component, AfterViewInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { MessageService } from '../../server/message.service';
import { Messages } from '../../server/structure'
import { Notice } from './notice';

@Component({
    selector: 'design-notice-area',
    templateUrl: './notice-area.component.html',
    styleUrls: ['./style.scss']
})
export class NoticeAreaComponent implements AfterViewInit, OnDestroy {
    private notices: Array<Notice> = new Array<Notice>();
    private messageListener: Subscription;

    constructor(public message: MessageService) { }

    public ngAfterViewInit(): void {
        this.messageListener = this.message.listen(m => {
            if (m.is(Messages.Notice)) {
                var target = m.read<Notice>();
                this.notices.splice(0, 0, target);
                setTimeout(() => {
                    if (this.notices.indexOf(target) == -1) return;
                    this.message.prepare().tag(Messages.PushNoticeHistory).value(target).go();
                    this.remove(target);
                }, 7500);
            }
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
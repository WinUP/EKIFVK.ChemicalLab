import { Injectable } from '@angular/core';
import { BehaviorSubject, } from 'rxjs/BehaviorSubject';
import { Subscription } from 'rxjs/Subscription';
import { Message } from './structure';
import { Configuration } from './configuration'

@Injectable()
export class MessageService {
    private initialMessage: Message = Message.from(this).tag('SystemStart').value(null);
    private messageSubject = new BehaviorSubject<Message>(this.initialMessage);
    private messageQueue = this.messageSubject.asObservable();

    constructor() {
        if (Configuration.functionProxy) {
            var _this = this;
            window['system_proxy'] = window['system_proxy'] || {};
            window['system_proxy']['message_send'] = function (tag, value) {
                Message.from(_this).tag(tag).value(value).go();
            };
        }
    }

    public prepare(): Message {
        return Message.from(this);
    }

    public send(data: Message): void {
        if (data == null) return;
        console.log(`----- [MessageService] ${data.readTag()} ->`);
        console.log(data.read<any>());
        this.messageSubject.next(data);
    }

    public listen(delegate: (m: Message) => void): Subscription {
        return this.messageQueue.subscribe(delegate);
    }
}
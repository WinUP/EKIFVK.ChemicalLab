import { Injectable } from '@angular/core';
import { EventListener } from './structure';

@Injectable()
export class SynchronizedEventService {
    private listeners: EventListener<any>[] = [];

    public listen(listener: EventListener<any>): void {
        if (this.listeners.indexOf(listener) < 0) this.listeners.push(listener);
    }

    public remove(listener: EventListener<any>): void {
        var index = this.listeners.indexOf(listener);
        if (index > -1) this.listeners.splice(index, 1);
    }

    public fire<T>(tag: string, data: T): T {
        console.log(`----- [SynchronizedEvent] FIRE ${tag} ->`);
        console.log(data);
        var result = data;
        this.listeners.forEach(element => {
             if (element.tag == tag) result = element.handler(result);
        });
        console.log(`----- [SynchronizedEvent] FINAL ${tag} ->`);
        console.log(result);
        return result;
    }
}
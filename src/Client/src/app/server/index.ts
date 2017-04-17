import { NgModule } from '@angular/core';
import { Http, XHRBackend, RequestOptions } from '@angular/http';
import { HttpService } from './http.service';
import { StorageService } from './storage.service';
import { MessageService } from './message.service';
import { SynchronizedEventService } from './synchronized-event.service'

export function httpFactory(xhrBackend, requestOptions, storage, message) {
    return new HttpService(xhrBackend, requestOptions, storage, message);
}

@NgModule({
    providers: [
        MessageService,
        StorageService,
        SynchronizedEventService,
        {
            provide: Http,
            useFactory: httpFactory,
            deps: [XHRBackend, RequestOptions, StorageService, MessageService]
        } 
    ]
})
export class ServerModule { }

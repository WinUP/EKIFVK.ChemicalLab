import { Injectable } from '@angular/core';
import { Http, Headers, Request, Response, RequestOptions, RequestOptionsArgs, ConnectionBackend } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import { ServerData, ServerMessage, Message, StorageType, LocalStorageKey } from './structure';
import { StorageService } from './storage.service';
import { MessageService } from './message.service';
import { Configuration } from './configuration'

@Injectable()
export class HttpService extends Http {
    constructor(backend: ConnectionBackend, defaultOptions: RequestOptions, private storage: StorageService, private message: MessageService) {
        super(backend, defaultOptions);
    }

    get(url: string, options?: RequestOptionsArgs): Observable<Response> {
        var op = this.decorate(options);
        url = Configuration.serverAddress + url;
        return this.intercept(super.get(url, op), 1, url, op);
    }

    post(url: string, body: string, options?: RequestOptionsArgs): Observable<Response> {
        var op = this.decorate(options);
         url = Configuration.serverAddress + url;
        return this.intercept(super.post(url, body, op), 2, url, op, body);
    }
 
    put(url: string, body: string, options?: RequestOptionsArgs): Observable<Response> {
        var op = this.decorate(options);
        url = Configuration.serverAddress + url;
        return this.intercept(super.put(url, body, op), 3, url, op, body);
    }
 
    delete(url: string, options?: RequestOptionsArgs): Observable<Response> {
        var op = this.decorate(options);
        url = Configuration.serverAddress + url;
        return this.intercept(super.delete(url, op), 4, url, op);
    }

    patch(url: string, body: string, options?: RequestOptionsArgs): Observable<Response> {
        var op = this.decorate(options);
        url = Configuration.serverAddress + url;
        return this.intercept(super.patch(url, body, op), 5, url, op, body);
    }

    private intercept(observable: Observable<Response>, type: 1 | 2 | 3 | 4 | 5, url: string, options?: RequestOptionsArgs, body?: string): Observable<Response> {
        return observable.catch((response: Response) => {
            var data: ServerData = response.json();
            if (response.status == 403 && data.message == ServerMessage.Authentication.OverdueString) {
                var username = this.storage.read<string>(StorageType.Local, LocalStorageKey.Username);
                var password = this.storage.read<string>(StorageType.Local, LocalStorageKey.Password);
                if (username == '' || username == null || password == '' || password == null)
                    return Observable.throw(this.parseError(response))
                return Configuration.refreshBy(super.patch, Configuration.serverAddress, username, password).mergeMap(value => {
                    var data: string = Configuration.getTokenBy(value);
                    if (data == ServerMessage.User.IncorrectPassword) throw value;
                    this.storage.local(LocalStorageKey.Token, data);
                    options.headers.set(Configuration.TokenHttpHeader, data);
                    switch (type) {
                        case 1:
                            return super.get(url, options);
                        case 2:
                            return super.post(url, body, options);
                        case 3:
                            return super.put(url, body, options);
                        case 4:
                            return super.patch(url, body, options);
                        case 5:
                            return super.delete(url, options);
                        default:
                            return Observable.throw(this.parseError(response));
                    }
                }).catch((e: Response) => {
                    return Observable.throw(this.parseError(e))
                });
            } else
                return Observable.throw(this.parseError(response));
        });
    }

    private parseError(e: Response): ServerData {
        if (e.status == 500)
            return { message: ServerMessage.Authentication.ServerError, data: null, code: 500 };
        var data: ServerData = e.json();
        data.code = e.status;
        return data;
    }

    private decorate(options?: RequestOptionsArgs) : RequestOptionsArgs {
        if (options == null) options = new RequestOptions();
        if (options.headers == null) options.headers = new Headers();
        options.headers.append('Content-Type', 'application/json');
        options.headers.append(Configuration.TokenHttpHeader, this.storage.read<string>(StorageType.Local, LocalStorageKey.Token));
        return options;
    }
}
import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { ServerData, ServerMessage, UserInformation, LocalStorageKey,
         SessionStorageKey, CacheStorageKey, ChangeUserInformation } from '../server/structure';
import { StorageService } from '../server/storage.service';
import { Observable } from 'rxjs';

@Injectable()
export class UserService {
    constructor(private http: Http, private storage: StorageService) { }

    public signIn(username: string, password: string): Observable<string> {
        return this.http.patch(`/user/${username}`, { accessToken: password })
        .map(v => {
            var data: string = v.json().data['accessToken'];
            if (data == ServerMessage.User.IncorrectPassword || data == ServerMessage.User.OperationDenied) {
                var error: ServerData = { message: data, data: null, code: v.status };
                throw error;
            } else {
                this.storage.local(LocalStorageKey.Token, data);
                return data;
            }  
        });
    }

    public signOut(username: string): Observable<string> {
        return this.http.patch(`/user/${username}`, { accessToken: false })
        .map(v => {
            var data: string = v.json().data['accessToken'];
            if (data == ServerMessage.User.OperationDenied) {
                var error: ServerData = { message: data, data: null, code: v.status };
                throw error;
            } else {
                this.storage.local(LocalStorageKey.Username, null);
                this.storage.local(LocalStorageKey.Password, null);
                this.storage.local(LocalStorageKey.Token, null);
                this.storage.session(SessionStorageKey.UserInformation, null);
                this.storage.cache(CacheStorageKey.CurrentCardAction, null);
                this.storage.cache(CacheStorageKey.PreviousCardAction, null);
                return data;
            }
        });
    }

    public getInfo(username: string): Observable<UserInformation> {
        return this.http.get(`/user/${username}`).map(v => v.json().data);
    }

    public changeInfo(username: string, data: ChangeUserInformation): Observable<ChangeUserInformation> {
        return this.http.patch(`/user/${username}`, data).map(v => {
            var response: ServerData = v.json();
            var data: ChangeUserInformation = response.data;
            response.code = 200;
            Object.keys(data).forEach(e => {
                if (data[e] == ServerMessage.User.OperationDenied) response.code = 403;
                if (data[e] == ServerMessage.Authentication.DeniedString) response.code = 403;
                if (data[e] == ServerMessage.Authentication.InvalidString) response.code = 403;
                if (data[e] == ServerMessage.User.CannotChangeSelf) response.code = 403;
                if (data[e] == ServerMessage.User.InvalidGroupName) response.code = 404;
                if (response.code != 200) throw response;
            });
            return data;
        });
    }
}
import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { ServerData, ServerMessage, UserInformation } from '../server/structure';
import { Observable } from 'rxjs';

@Injectable()
export class UserService {
    constructor(private http: Http) { }

    public signIn(username: string, password: string): Observable<string> {
        return this.http.patch(`/user/${username}`, { accessToken: password })
        .map(v => {
            var data: string = v.json().data['accessToken'];
            if (data == ServerMessage.User.IncorrectPassword || data == ServerMessage.User.OperationDenied) {
                var error: ServerData = { message: data, data: null, code: v.status };
                throw error;
            } else
                return data;
        });
    }

    public signOut(username: string): Observable<string> {
        return this.http.patch(`/user/${username}`, { AccessToken: false })
        .map(v => {
            var data: string = v.json().data['accessToken'];
            if (data == ServerMessage.User.OperationDenied) {
                var error: ServerData = { message: data, data: null, code: v.status };
                throw error;
            } else
                return data;
        });
    }

    public getInfo(username: string): Observable<UserInformation> {
        return this.http.get(`/user/${username}`).map(v => v.json().data);
    }
}
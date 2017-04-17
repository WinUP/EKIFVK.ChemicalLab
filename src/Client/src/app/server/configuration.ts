import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs/Observable';

export var Configuration = {
    functionProxy: true,
    serverAddress: 'http://localhost:54205/api/1.1',
    localStorageRoot: 'ChemicalLab',
    TokenHttpHeader: 'X-Access-Token',
    refreshBy: function (method: (url: string, body?: any) => Observable<Response>, server: string, username: string, password: string): Observable<Response> {
        return method(`${server}/user/${username}`, { AccessToken: password });
    },
    getTokenBy: function (response: Response): string {
        return response.json()['AccessToken'];
    }
};
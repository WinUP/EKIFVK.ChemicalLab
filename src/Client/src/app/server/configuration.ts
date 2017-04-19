import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs/Observable';

export var Configuration = {
    functionProxy: true,
    serverAddress: 'http://localhost:5000/api/1.1',
    localStorageRoot: 'ChemicalLab',
    sessionStorageRoot: 'ChemicalLab_SESSION',
    TokenHttpHeader: 'X-Access-Token',
    refreshBy: function (method: (url: string, body?: any) => Observable<Response>, server: string, username: string, password: string): Observable<Response> {
        return method(`${server}/user/${username}`, { accessToken: password });
    },
    getTokenBy: function (response: Response): string {
        return response.json()['accessToken'];
    }
};
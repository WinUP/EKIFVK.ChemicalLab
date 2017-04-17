import { Component } from '@angular/core';
import { UserService } from '../user.service'
import { Messages, ServerData, LocalData, LocalStorageKey, SessionStorageKey, 
         ServerMessage, UserInformation } from '../../server/structure'
import { StorageService } from '../../server/storage.service'
import { MessageService } from '../../server/message.service'
import { Notice } from '../../design-support/notice/notice'
import * as Crypto from 'crypto-js'

@Component({
    selector: 'system-user-signin',
    templateUrl: './signin.component.html',
    styleUrls: ['./signin.component.scss']
})
export class SigninComponent {
    public name: string = '';
    public password: string = '';
    public errors = { name: '', password: '' }

    constructor(public message: MessageService, public storage: StorageService, public user: UserService) { }

    public signIn(): void {
        this.errors.name = this.errors.password = '';
        if (this.name == '') {
            this.errors.name = 'This cannot be empty';
            return;
        }
        if (this.password == '') {
            this.errors.password = 'This cannot be empty';
            return;
        }
        var passwordHash = (Crypto.SHA256(this.password) + '').toUpperCase();
        this.user.signIn(this.name, passwordHash).subscribe(token => {
            this.storage.local(LocalStorageKey.Username, this.name);
            this.storage.local(LocalStorageKey.Password, passwordHash);
            this.storage.local(LocalStorageKey.Token, token);
            this.message.prepare().tag(Messages.Notice).value<Notice>({
                icon: 'account_box',
                title: `Hello`,
                time: new Date(),
                content: `${this.name}, welcome! We are trying to get your information.`
            }).go();
            this.updateUserInformation();
            this.name =  this.password = '';
        }, (error: ServerData) => {
            if (error.message == ServerMessage.User.IncorrectPassword)
                this.errors.password = 'Wrong password';
            else if (error.message == ServerMessage.User.InvalidUserName)
                this.errors.name = 'We cannot find this account';
            else if (error.message == ServerMessage.User.OperationDenied)
                this.errors.name = 'This account is disabled';
        });
    }

    public updateUserInformation(): void {
        this.user.getInfo(this.name).subscribe(info => {
            this.message.prepare().tag(Messages.Notice).value<Notice>({
                icon: 'account_box',
                title: `System notice`,
                time: new Date(),
                content: 'Your information is here, enjoy!'
            }).go();
            this.storage.session(SessionStorageKey.UserInformation, info);
        }, (error: ServerData) => {
            var notice: string = 'Request was rejected by server';
            if (error.message == ServerMessage.Authentication.InvalidString)
                notice = 'Cannot verify your identity';
            this.message.prepare().tag(Messages.Notice).value<Notice>({
                icon: 'account_box',
                title: `Error`,
                time: new Date(),
                content: 'We cannot get your info: ' + notice
            }).go();
        });
    }
}

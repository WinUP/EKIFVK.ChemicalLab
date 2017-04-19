import { Component, ViewChild, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router'
import { MdSidenav } from '@angular/material'
import { Subscription } from 'rxjs/Subscription';
import { PanelActionComponent } from './design-support/panel/panel-action.component';
import { ServerData, Messages, LocalData, StorageType, SessionStorageKey,
         CacheStorageKey, LocalStorageKey, UserInformation, ServerMessage } from './server/structure';
import { StorageService } from './server/storage.service';
import { MessageService } from './server/message.service';
import { UserService } from './user/user.service';
import { Notice } from './design-support/notice/notice'

@Component({
    selector: 'system-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss']
})
export class ApplicationComponent implements OnInit, AfterViewInit, OnDestroy {
    @ViewChild(MdSidenav) public sidenav: MdSidenav;
    private messageListener: Subscription;
    private showNotice: boolean = false;
    private pageTitle: string = '';
    private noticeCount:number = 0;
    private userInformation: UserInformation = null;
    private path: string = window.location.pathname;

    constructor(public storage: StorageService, public message: MessageService, public user: UserService, public router: Router) { }

    public ngOnInit(): void {
        this.userInformation = this.storage.read<UserInformation>(StorageType.Session, SessionStorageKey.UserInformation);
        if (this.userInformation == null) this.router.navigate(['/signin']);
        this.path = this.userInformation == null ? '/signin' : this.path;
        if (this.path == '/signin' && this.userInformation != null) {
            this.path = '/dashboard';
            this.router.navigate(['/dashboard']);
        }
        var index = this.path.indexOf('/', 1);
        if (index > -1)
            this.path = this.path.substring(1, this.path.indexOf('/', 1))
        else
            this.path = this.path.substring(1);
        if (this.path == 'signin') this.pageTitle = 'SIGN IN';
        else if (this.path == 'dashboard') this.pageTitle = 'DASHBOARD';
        else if (this.path == 'profile') this.pageTitle = 'PROFILE';
        this.preInitAction();
    }

    public ngAfterViewInit(): void {
        this.messageListener = this.message.listen(m => {
            if (m.is(Messages.Storage)) {
                var data = m.read<LocalData>();
                if (data.type == StorageType.Session && data.key == SessionStorageKey.UserInformation) {
                    this.userInformation = data.value;
                }
            } 
            else if (m.is(Messages.PushNoticeHistory))
                this.noticeCount ++;
            else if (m.is(Messages.CardActionClick))
                this.navigate(m.read<string>());
        });
    }

    public ngOnDestroy(): void {
        this.messageListener.unsubscribe();
    }

    public preInitAction(): void {
        var _path = () => this.path;
        this.initAction = function (action: PanelActionComponent): void {
            if (action.action == 'a_signin' && _path() == 'signin') action.toggleActive();
            if (action.action == 'a_dashboard' && _path() == 'dashboard') action.toggleActive();
            if (action.action == 'a_profile' && _path() == 'profile') action.toggleActive();
        };
    };

    public initAction: (action: PanelActionComponent) => void;

    public toggleNotice(): void {
        this.noticeCount = 0;
        this.showNotice = !this.showNotice;
    }

    public signOut(): void {
        this.message.prepare().tag(Messages.Notice).value<Notice>({
                icon: 'account_box',
                title: `Sign out`,
                time: new Date(),
                content: 'We are trying to sign out your account...'
            }).go();
        this.user.signOut(this.storage.read<string>(StorageType.Local, LocalStorageKey.Username)).subscribe(v => {
            this.navigate('a_signin');
            this.message.prepare().tag(Messages.Notice).value<Notice>({
                icon: 'account_box',
                title: `Sign out`,
                time: new Date(),
                content: `Your account information was removed from this browser successfully.`
            }).go();
        }, (error: ServerData) => {
            console.log(error);
            var notice: string = 'Request was rejected by server';
            if (error.message == ServerMessage.User.OperationDenied)
                notice = 'You can only sign out yourself'
            else if (error.message == ServerMessage.Authentication.InvalidString)
                notice = 'Cannot verify your identity';
            this.message.prepare().tag(Messages.Notice).value<Notice>({
                icon: 'account_box',
                title: `Error`,
                time: new Date(),
                content: 'We cannot sign out your account: ' + notice
            }).go();
        });
    }

    public navigate(action: string): void {
        this.sidenav.close();
        var path = '';
        if (action == 'a_signin') {
            path = '/signin';
            this.path = 'signin';
            this.pageTitle = 'SIGN IN';
        }
        else if (action == 'a_signout')
            this.signOut();
        else if (action == 'a_dashboard') {
            path = '/dashboard';
            this.path = 'dashboard';
            this.pageTitle = 'DASHBOARD';
        }
        else if (action == 'a_profile') {
            path = '/profile';
            this.path = 'profile';
            this.pageTitle = 'PROFILE';
        }
        if (path != '') this.router.navigate([path]);
    }

    public newTab(): void {
        window.open(window.location.href, '_blank');
    }
}
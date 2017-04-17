import { Component, ViewChild, AfterViewInit, AfterContentInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router'
import { MdSidenav } from '@angular/material'
import { Subscription } from 'rxjs/Subscription';
import { PanelActionComponent } from './design-support/panel/panel-action.component';
import { ServerData, Message, Messages, LocalData, StorageType, SessionStorageKey,
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
export class ApplicationComponent implements AfterViewInit, AfterContentInit, OnDestroy {
    @ViewChild(MdSidenav) public sidenav: MdSidenav;
    private messageListener: Subscription;
    private showNotice: boolean = false;
    private pageTitle: string = '';
    private noticeCount:number = 0;
    private userInformation: UserInformation = null;

    constructor(public storage: StorageService, public message: MessageService, public user: UserService, public router: Router) { }

    public ngAfterViewInit(): void {
        this.messageListener = this.message.listen(m => {
            if (m.is(Messages.Storage)) {
                var data = m.read<LocalData>();
                if (data.type == StorageType.Session && data.key == SessionStorageKey.UserInformation) {
                     this.userInformation = data.value;
                     if (this.userInformation != null) {
                         this.pageTitle = 'Overview';
                         this.router.navigate(['/overview']);
                     }
                }
            } 
            else if (m.is(Messages.PushNoticeHistory))
                this.noticeCount ++;
            else if (m.is(Messages.CardActionClick))
                this.navigate(m.read<PanelActionComponent>());
        });
    }

    public ngAfterContentInit(): void {
        this.userInformation = this.storage.read<UserInformation>(StorageType.Session, SessionStorageKey.UserInformation);
        if (this.userInformation == null) this.router.navigate(['/signin']);
    }

    public ngOnDestroy(): void {
        this.messageListener.unsubscribe();
    }

    public toggleNotice(): void {
        if (!this.showNotice) this.noticeCount = 0;
        this.showNotice = !this.showNotice;
    }

    public signOut(): void {
        this.user.signOut(this.storage.read<string>(StorageType.Local, LocalStorageKey.Username)).subscribe(v => {
            this.storage.local(LocalStorageKey.Username, null);
            this.storage.local(LocalStorageKey.Password, null);
            this.storage.local(LocalStorageKey.Token, null);
            this.storage.session(SessionStorageKey.UserInformation, null);
            this.storage.cache(CacheStorageKey.CurrentCardAction, null);
            this.storage.cache(CacheStorageKey.PreviousCardAction, null);
            this.message.prepare().tag(Messages.Notice).value<Notice>({
                icon: 'account_box',
                title: `Account`,
                time: new Date(),
                content: `Sign out successful`
            }).go();
            this.router.navigate(['/signin']);
        }, (error: ServerData) => {
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

    public navigate(action: PanelActionComponent): void {
        this.sidenav.close();
        var path = '';
        if (action.title == 'Sign out') this.signOut();
        else if (action.title == 'Ovewview') {
            path = '/overview';
            this.pageTitle = 'Ovewvier';
        }
        else if (action.title == 'Profile') {
            path = '/profile';
            this.pageTitle = 'Profile';
        }
        if (path != '') this.router.navigate([path]);
    }

    public initAction(action: PanelActionComponent): void {
        var path = window.location.pathname;
        if (path == '/')
            path = this.userInformation == null ? '/signin' : '/overview';
        var index = path.indexOf('/', 1);
        if (index > -1) path = path.substring(1, path.indexOf('/', 1))
        else path = path.substring(1);
        if (action.title == 'Overview' && path == 'overview') action.toggleActive();
    }
}
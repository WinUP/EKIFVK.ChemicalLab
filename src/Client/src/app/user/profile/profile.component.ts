import { Component, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { MdDialog } from '@angular/material';
import { Subscription } from 'rxjs/Subscription';
import { UserInformation, StorageType, LocalStorageKey, SessionStorageKey,
         Messages, LocalData, ServerData, ServerMessage } from '../../server/structure';
import { StorageService } from '../../server/storage.service';
import { MessageService } from '../../server/message.service';
import { UserService } from '../user.service';
import { Notice } from '../../design-support/notice/notice';
import { PasswordConfirmDialogComponent } from './password-confirm-dialog/password-confirm-dialog.component';
import * as Crypto from 'crypto-js';

@Component({
    selector: 'system-user-profile',
    templateUrl: './profile.component.html',
    styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit, AfterViewInit, OnDestroy {
    private info: UserInformation;
    public password: string = '';
    public errors = { password: '', displayName: '' };
    private permission = {
        User: [
            { key: 'UGADD', value: false, note: 'Add new group' },
            { key: 'UGDEL', value: false, note: 'Delete group' },
            { key: 'UGMAN', value: false, note: 'View group detail informatiom' },
            { key: 'UGPER', value: false, note: 'Change group permission' },
            { key: 'UGDIS', value: false, note: 'Disabled or enable group' },
            { key: 'USADD', value: false, note: 'Add new user' },
            { key: 'USDEL', value: false, note: 'Delete user' },
            { key: 'USMAN', value: false, note: 'View user detail information' },
            { key: 'USMOD', value: false, note: 'Change user information' },
            { key: 'USGRP', value: false, note: 'Change user group' },
            { key: 'USDIS', value: false, note: 'Disable or enable user' }
        ],
        History: [
            { key: 'TKGET', value: false, note: 'View tracked histories' },
            { key: 'TKDEL', value: false, note: 'Delete traching record' }
        ],
        Lab: [
            { key: 'CTADD', value: false, note: 'Add new container type' },
            { key: 'CTDEL', value: false, note: 'Delete container type' },
            { key: 'CTMAN', value: false, note: 'Change containter type information' },
            { key: 'EXADD', value: false, note: 'Add new experiment' },
            { key: 'EXDEL', value: false, note: 'Delete experiment' },
            { key: 'EXMAN', value: false, note: 'Change experiment information' },
            { key: 'IDADD', value: false, note: 'Add new chemical item detail' },
            { key: 'IDDEL', value: false, note: 'Delete chemical item detail' },
            { key: 'IDMAN', value: false, note: 'Change chemical item detail information' },
            { key: 'ITADD', value: false, note: 'Add new chemical item' },
            { key: 'ITDEL', value: false, note: 'Delete chemical item' },
            { key: 'ITMAN', value: false, note: 'Change chemical item information' },
            { key: 'DTADD', value: false, note: 'Add new chemical item detail type' },
            { key: 'DTDEL', value: false, note: 'Delete chemical item detail type' },
            { key: 'DTMAN', value: false, note: 'Change chemical item detail type information' },
            { key: 'MSMAN', value: false, note: 'View all MSDS file in server' },
            { key: 'MSDEL', value: false, note: 'Delete MSDS file' },
            { key: 'PSADD', value: false, note: 'Add new physical state' },
            { key: 'PSDEL', value: false, note: 'Delete physical state' },
            { key: 'PSMAN', value: false, note: 'Change physical state information' },
            { key: 'UNADD', value: false, note: 'Add new unit' },
            { key: 'UNDEL', value: false, note: 'Delete unit' },
            { key: 'UNMAN', value: false, note: 'Change unit information' },
            { key: 'VDADD', value: false, note: 'Add new vendor' },
            { key: 'VDDEL', value: false, note: 'Delete vendor' },
            { key: 'VDMAN', value: false, note: 'Change vendor information' }
        ],
        Storage: [
            { key: 'SRADD', value: false, note: 'Add new room, place or location' },
            { key: 'SRDEL', value: false, note: 'Delete room, place or location' },
            { key: 'SRMAN', value: false, note: 'Change room, place or location information' }
        ]
    };
    private messageListener: Subscription;

    constructor(public message: MessageService, public storage: StorageService, public user: UserService, public dialog: MdDialog) { }

    public ngOnInit(): void {
        this.parseInformarion(this.storage.read<UserInformation>(StorageType.Session, SessionStorageKey.UserInformation));
    }

    public ngAfterViewInit(): void {
        this.messageListener = this.message.listen(m => {
            if (m.is(Messages.Storage)) {
                var data = m.read<LocalData>();
                if (data.type == StorageType.Session && data.key == SessionStorageKey.UserInformation) {
                      this.parseInformarion(data.value);
                }
            }
        });
    }

    public ngOnDestroy(): void {
        this.messageListener.unsubscribe();
    }

    private parseInformarion(info: UserInformation): void {
        this.info = info;
        if (this.info == null) return;
        this.permission.History[0].value = this.info.permission.indexOf('TK:GET') > -1;
        this.permission.History[1].value = this.info.permission.indexOf('TK:DELETE') > -1;
        this.permission.Storage[0].value = this.info.permission.indexOf('SR:ADD') > -1;
        this.permission.Storage[1].value = this.info.permission.indexOf('SR:DELETE') > -1;
        this.permission.Storage[2].value = this.info.permission.indexOf('SR:MANAGE') > -1;
        this.permission.User[0].value = this.info.permission.indexOf('UG:ADD') > -1;
        this.permission.User[1].value = this.info.permission.indexOf('UG:DELETE') > -1;
        this.permission.User[2].value = this.info.permission.indexOf('UG:MANAGE') > -1;
        this.permission.User[3].value = this.info.permission.indexOf('UG:PERM') > -1;
        this.permission.User[4].value = this.info.permission.indexOf('UG:DISABLE') > -1;
        this.permission.User[5].value = this.info.permission.indexOf('US:ADD') > -1;
        this.permission.User[6].value = this.info.permission.indexOf('US:DELETE') > -1;
        this.permission.User[7].value = this.info.permission.indexOf('US:MANAGE') > -1;
        this.permission.User[8].value = this.info.permission.indexOf('US:MODIFY') > -1;
        this.permission.User[9].value = this.info.permission.indexOf('US:GROUP') > -1;
        this.permission.User[10].value = this.info.permission.indexOf('US:DISABLE') > -1;
        this.permission.Lab[0].value = this.info.permission.indexOf('CT:ADD') > -1;
        this.permission.Lab[1].value = this.info.permission.indexOf('CT:DELETE') > -1;
        this.permission.Lab[2].value = this.info.permission.indexOf('CT:MANAGE') > -1;
        this.permission.Lab[3].value = this.info.permission.indexOf('EX:ADD') > -1;
        this.permission.Lab[4].value = this.info.permission.indexOf('EX:DELETE') > -1;
        this.permission.Lab[5].value = this.info.permission.indexOf('EX:MANAGE') > -1;
        this.permission.Lab[6].value = this.info.permission.indexOf('ID:ADD') > -1;
        this.permission.Lab[7].value = this.info.permission.indexOf('ID:DELETE') > -1;
        this.permission.Lab[8].value = this.info.permission.indexOf('ID:MANAGE') > -1;
        this.permission.Lab[9].value = this.info.permission.indexOf('IT:ADD') > -1;
        this.permission.Lab[10].value = this.info.permission.indexOf('IT:DELETE') > -1;
        this.permission.Lab[11].value = this.info.permission.indexOf('IT:MANAGE') > -1;
        this.permission.Lab[12].value = this.info.permission.indexOf('DT:ADD') > -1;
        this.permission.Lab[13].value = this.info.permission.indexOf('DT:DELETE') > -1;
        this.permission.Lab[14].value = this.info.permission.indexOf('DT:MANAGE') > -1;
        this.permission.Lab[15].value = this.info.permission.indexOf('MS:MANAGE') > -1;
        this.permission.Lab[16].value = this.info.permission.indexOf('MS:DELETE') > -1;
        this.permission.Lab[17].value = this.info.permission.indexOf('PS:ADD') > -1;
        this.permission.Lab[18].value = this.info.permission.indexOf('PS:DELETE') > -1;
        this.permission.Lab[19].value = this.info.permission.indexOf('PS:MANAGE') > -1;
        this.permission.Lab[20].value = this.info.permission.indexOf('UN:ADD') > -1;
        this.permission.Lab[21].value = this.info.permission.indexOf('UN:DELETE') > -1;
        this.permission.Lab[22].value = this.info.permission.indexOf('UN:MANAGE') > -1;
        this.permission.Lab[23].value = this.info.permission.indexOf('VD:ADD') > -1;
        this.permission.Lab[24].value = this.info.permission.indexOf('VD:DELETE') > -1;
        this.permission.Lab[25].value = this.info.permission.indexOf('VD:MANAGE') > -1;
    }

    public changePassword(): void {
        if (this.password == '') {
            this.errors.password = 'This cannot be empty';
            return;
        }
        this.dialog.open(PasswordConfirmDialogComponent, { data: this.password })
        .afterClosed().subscribe((r: boolean) => {
            if (!r) return;
            var passwordHash = (Crypto.SHA256(this.password) + '').toUpperCase();
            this.user.changeInfo(this.info.name, { password: passwordHash }).subscribe(v => {
                this.storage.local(LocalStorageKey.Password, passwordHash);
                this.message.prepare().tag(Messages.Notice).value<Notice>({
                    icon: 'account_box',
                    title: `Password changed`,
                    time: new Date(),
                    content: `We are refreshing your new information.`
                }).go();
                this.updateUserInformation();
            }, (error: ServerData) => {
                var notice: string = 'Request was rejected by server';
                if (error.data.password == ServerMessage.Authentication.InvalidString)
                    notice = 'Cannot verify your identity';
                this.message.prepare().tag(Messages.Notice).value<Notice>({
                    icon: 'account_box',
                    title: `Error`,
                    time: new Date(),
                    content: 'We cannot change your password: ' + notice
                }).go();
            });
        })
    }

    public changeDisplayName(): void {
        if (this.info.displayName == '') {
            this.errors.displayName = 'This cannot be empty';
            return;
        }
        this.user.changeInfo(this.info.name, { displayName: this.info.displayName }).subscribe(v => {
            this.message.prepare().tag(Messages.Notice).value<Notice>({
                icon: 'account_box',
                title: `Display name changed`,
                time: new Date(),
                content: `We are refreshing your new information.`
            }).go();
            this.updateUserInformation();
        }, (error: ServerData) => {
            var notice: string = 'Request was rejected by server';
            if (error.data.password == ServerMessage.Authentication.InvalidString)
                notice = 'Cannot verify your identity';
            this.message.prepare().tag(Messages.Notice).value<Notice>({
                icon: 'account_box',
                title: `Error`,
                time: new Date(),
                content: 'We cannot change your display name: ' + notice
            }).go();
        });
    }

    public updateUserInformation(): void {
        this.user.getInfo(this.info.name).subscribe(info => {
            this.message.prepare().tag(Messages.Notice).value<Notice>({
                icon: 'account_box',
                title: `Information updated`,
                time: new Date(),
                content: 'Your new information was recorded by system.'
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

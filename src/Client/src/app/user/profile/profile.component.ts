import { Component, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { UserInformation, StorageType, SessionStorageKey, Messages, LocalData } from '../../server/structure';
import { StorageService } from '../../server/storage.service';
import { MessageService } from '../../server/message.service';

@Component({
    selector: 'system-user-profile',
    templateUrl: './profile.component.html',
    styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit, AfterViewInit, OnDestroy {
    private info: UserInformation;
    private messageListener: Subscription;

    constructor(public message: MessageService, public storage: StorageService) { }

    public ngOnInit(): void {
        this.info = this.storage.read<UserInformation>(StorageType.Session, SessionStorageKey.UserInformation);
    }

    public ngAfterViewInit(): void {
        this.messageListener = this.message.listen(m => {
            if (m.is(Messages.Storage)) {
                var data = m.read<LocalData>();
                if (data.type == StorageType.Session && data.key == SessionStorageKey.UserInformation) {
                     this.info = data.value;
                }
            }
        });
    }

    public ngOnDestroy(): void {
        this.messageListener.unsubscribe();
    }

}

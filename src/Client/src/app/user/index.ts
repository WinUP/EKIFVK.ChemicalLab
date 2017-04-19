import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { MdInputModule, MdButtonModule, MdIconModule, MdListModule } from '@angular/material';
import { DesignSupportModule } from '../design-support'
import { ServerModule } from '../server';
import { SigninComponent } from './signin/signin.component';
import { ProfileComponent } from './profile/profile.component';
import { PasswordConfirmDialogComponent } from './profile/password-confirm-dialog/password-confirm-dialog.component';
import { UserService } from './user.service';

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        HttpModule,
        MdInputModule,
        MdButtonModule,
        MdIconModule,
        MdListModule,
        DesignSupportModule,
        ServerModule
    ],
    declarations: [
        SigninComponent,
        ProfileComponent,
        PasswordConfirmDialogComponent
    ],
    exports: [
        SigninComponent,
        ProfileComponent
    ],
    entryComponents: [ PasswordConfirmDialogComponent ],
    providers: [ UserService ]
})
export class UserModule { }

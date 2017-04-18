import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { MdInputModule, MdButtonModule, MdIconModule } from '@angular/material';
import { DesignSupportModule } from '../design-support'
import { ServerModule } from '../server';
import { SigninComponent } from './signin/signin.component';
import { ProfileComponent } from './profile/profile.component';
import { UserService } from './user.service';

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        HttpModule,
        MdInputModule,
        MdButtonModule,
        MdIconModule,
        DesignSupportModule,
        ServerModule
    ],
    declarations: [
        SigninComponent,
        ProfileComponent
    ],
    exports: [
        SigninComponent,
        ProfileComponent
    ],
    providers: [ UserService ]
})
export class UserModule { }

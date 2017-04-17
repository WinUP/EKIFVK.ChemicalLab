import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { MaterialModule } from '@angular/material';
import { DesignSupportModule } from '../design-support'
import { ServerModule } from '../server';
import { SigninComponent } from './signin/signin.component';
import { UserService } from './user.service'

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        HttpModule,
        MaterialModule,
        DesignSupportModule,
        ServerModule
    ],
    declarations: [
        SigninComponent
    ],
    exports: [
        SigninComponent
    ],
    providers: [ UserService ]
})
export class UserModule { }

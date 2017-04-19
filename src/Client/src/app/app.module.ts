import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations'
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { MaterialModule } from '@angular/material';
import { AppRoutingModule } from './app-routing.module';
import { ServerModule } from './server';
import { DesignSupportModule } from './design-support';
import { UserModule } from './user';
import { ApplicationComponent } from './app.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import 'hammerjs';
import './rxjs-extensions';

@NgModule({
    declarations: [ ApplicationComponent, DashboardComponent ],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        FormsModule,
        HttpModule,
        MaterialModule,
        AppRoutingModule,
        ServerModule,
        DesignSupportModule,
        UserModule
    ],
    providers: [ ],
    bootstrap: [ ApplicationComponent ]
})
export class ApplicationModule { }

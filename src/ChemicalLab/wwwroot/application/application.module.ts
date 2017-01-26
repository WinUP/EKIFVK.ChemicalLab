import { NgModule } from '@angular/core'
import { BrowserModule } from '@angular/platform-browser'
import { RouterModule } from '@angular/router'
import { HttpModule } from '@angular/http'
import { FormsModule } from '@angular/forms'
import { MaterialModule } from '@angular/material'
import { ApplicationComponent } from './application.component'
import 'hammerjs'

import { AppRoutingModule } from './app-routing/app-routing.module'
import { LoginComponent } from './login/login.component'
import { MainComponent } from './main/main.component'

@NgModule({
    imports: [
        BrowserModule,
        FormsModule,
        HttpModule,
        MaterialModule.forRoot(),
        AppRoutingModule
    ],
    providers: [],
    declarations: [
        ApplicationComponent,
        LoginComponent,
        MainComponent
    ],
    bootstrap: [ApplicationComponent]
})
export class ApplicationModule {
    // Nothing...
}
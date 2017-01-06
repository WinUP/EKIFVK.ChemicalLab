import { NgModule } from '@angular/core'
import { BrowserModule } from '@angular/platform-browser'
import { RouterModule } from '@angular/router'
import { HttpModule } from '@angular/http'
import { FormsModule }   from '@angular/forms'
import { MaterialModule } from '@angular/material'
import { ApplicationComponent } from './application.component'
import 'hammerjs'

@NgModule({
    imports: [MaterialModule.forRoot()],
    providers: [],
    declarations: [ApplicationComponent],
    bootstrap: [ApplicationComponent]
})
export class ApplicationModule {
    // Nothing...
 }
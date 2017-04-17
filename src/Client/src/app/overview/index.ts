import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { MaterialModule } from '@angular/material';
import { DesignSupportModule } from '../design-support'
import { ServerModule } from '../server';
import { OverviewComponent } from './overview.component'

@NgModule({
    imports: [
        CommonModule,
        MaterialModule,
        DesignSupportModule,
        ServerModule
    ],
    declarations: [
        OverviewComponent
    ],
    exports: [
        OverviewComponent
    ],
    providers: [ ]
})
export class OverviewModule { }

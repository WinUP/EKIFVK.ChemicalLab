import { CommonModule } from '@angular/common'
import { NgModule } from '@angular/core';
import { MdIconModule } from '@angular/material'
import { PanelComponent } from './panel/panel.component'
import { PanelActionComponent } from './panel/panel-action.component'
import { NoticeHistoryComponent } from './notice/notice-history.component'
import { NoticeAreaComponent } from './notice/notice-area.component'
import { DialogComponent } from './dialog/dialog.component'
import { TreeComponent } from './tree/tree.component';

@NgModule({
    imports: [
        CommonModule,
        MdIconModule
    ],
    declarations: [
        PanelComponent,
        PanelActionComponent,
        NoticeHistoryComponent,
        NoticeAreaComponent,
        DialogComponent,
        TreeComponent,
    ],
    exports: [
        PanelComponent,
        PanelActionComponent,
        NoticeHistoryComponent,
        NoticeAreaComponent,
        DialogComponent,
        TreeComponent,
    ]
})
export class DesignSupportModule { }

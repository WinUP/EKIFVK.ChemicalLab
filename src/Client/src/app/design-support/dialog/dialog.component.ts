import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'design-dialog',
    templateUrl: './dialog.component.html',
    styleUrls: ['./style.scss'],
})
export class DialogComponent {
    @Input() public title: string = '';
    @Output() public close: EventEmitter<any> = new EventEmitter();
}
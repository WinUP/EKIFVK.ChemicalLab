import { Component, Input } from '@angular/core';

@Component({
    selector: 'design-panel',
    templateUrl: './panel.component.html',
    styleUrls: ['./style.scss']
})
export class PanelComponent {
    @Input() public title: string;
    @Input() public icon: string;
}
import { Component, Input, Output, AfterContentInit, trigger, state, style, transition, animate } from '@angular/core';
import { MessageService } from '../../server/message.service'
import { Messages } from '../../server/structure'
import { Tree } from './tree'

@Component({
    selector: 'design-tree',
    templateUrl: './tree.component.html',
    styleUrls: ['./style.scss'],
    animations: [
        trigger('expandState', [
            state('hide', style({
                height: '0',
                marginTop: '0'
            })),
            state('expand', style({
                height: 'auto',
                marginTop: '5px'
            })),
            transition('hide => expand', animate('150ms ease-in')),
            transition('expand => hide', animate('150ms ease-out'))
        ])
    ]
})
export class TreeComponent implements AfterContentInit {
    private expandState: string = 'hide';
    private title: string;
    @Input() public prefix: string = '';
    @Input() public data: Tree;
    @Input() public hideTitle: boolean = false;
    @Input() public bordered: boolean = true;
    @Input() public opened: boolean = false;

    constructor(public message: MessageService) { }

    public ngAfterContentInit(): void {
        if (this.opened) this.toggleExpand();
    }

    public onItemClick(e: number): void {
        this.message.prepare().tag(Messages.TreeNodeClick).value(this.prefix + e).go;
    }

    public toggleExpand(): void {
        this.expandState = this.expandState == 'hide' ? 'expand' : 'hide';
    }
}

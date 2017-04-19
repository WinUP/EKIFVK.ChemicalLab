import { Component } from '@angular/core';
import { MdDialogRef } from '@angular/material';

@Component({
    selector: 'system-password-confirm-dialog',
    templateUrl: './password-confirm-dialog.component.html',
    styleUrls: ['./password-confirm-dialog.component.scss']
})
export class PasswordConfirmDialogComponent {
    public password: string = '';
    public confirmPassword: string = ''
    public errors = { password: '' };

    constructor(public dialogRef: MdDialogRef<PasswordConfirmDialogComponent>) { 
        this.password = dialogRef._containerInstance.dialogConfig.data;
    }

    public cancel(): void {
        this.dialogRef.close(false);
    }

    public confirm(): void {
        if (this.confirmPassword == '') {
            this.errors.password = 'This cannot be empty';
            return;
        }
        if (this.confirmPassword != this.password) {
            this.errors.password = 'Two passwords are not equals'
            return;
        }
        this.dialogRef.close(true);
    }
}

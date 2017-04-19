import { MessageService } from './message.service'

export class Message {
    private _service: MessageService = null;
    private _tag: string;
    private _value: any;

    private constructor() { }

    public static from(service: MessageService): Message {
        var target = new Message();
        target._service = service;
        return target;
    }

    public tag(tag: string): Message {
        this._tag = tag;
        return this;
    }

    public is(tag: string): boolean {
        return this._tag == tag;
    }

    public value<T>(value: T): Message {
        this._value = value;
        return this;
    }

    public read<T>(): T {
        return this._value;
    }

    public go(): void {
        if (this._service == null) return;
        this._service.send(this);
    }

    public readTag(): string {
        return this._tag;
    }
}

export const Messages = {
    Storage: 'Storage',
    Notice: 'Notice',
    PushNoticeHistory: 'PushNoticeHistory',
    CardActionClick: 'CardActionClick',
    TreeNodeClick: 'TreeNodeClick',
    UpdateUserInformation: 'UpdateUserInformation'
};

export const LocalStorageKey = {
    Username: 'username',
    Password: 'password',
    Token: 'token'
};

export const SessionStorageKey = {
    UserInformation: 'UserInformation'
};

export const CacheStorageKey = {
    PreviousCardAction: 'PreviousCardAction',
    CurrentCardAction: 'CurrentCardAction'
};

export enum StorageType {
    Local = 1,
    Session = 2,
    Cache = 3
};

export interface LocalData {
    key: string;
    value: any;
    type: StorageType;
}

export interface ServerData {
    message: string;
    data: any;
    code?: number;
}

export const ServerMessage = {
    Authentication: {
        AutorizedString: "VERIFY_AUTHORIZED",
        DeniedString: "VERIFY_DENIED",
        InvalidString: "VERIFY_INVALID",
        OverdueString: "VERIFY_OVERDUE",
        ServerError: "SERVER_ERROR"
    },
    User: {
        InvalidUserName: "INVALID_USER_NAME",
        InvalidGroupName: "INVALID_GROUP_NAME",
        IncorrectPassword: "INCORRECT_PASSWORD",
        AlreadyExisted: "ALREADY_EXISTED",
        CannotRemoveSelf: "CANNOT_REMOVE_SELF",
        CannotChangeSelf: "CANNOT_CHANGE_SELF",
        OperationDenied: "OPERATION_DENIED",
    },
    Place: {
        InvalidRoom: "INVALID_ROOM",
        InvalidPlace: "INVALID_PLACE",
        InvalidLocation: "INVALID_LOCATION",
        AlreadyExisted: "ALREADY_EXISTED",
        OperationDenied: "OPERATION_DENIED"
    },
    Lab: {
	    InvalidUnit: "INVALID_UNIT",
	    InvalidPhysicalState: "INVALID_PHYSICAL_STATE",
	    InvalidDetailType: "INVALID_DETAIL_TYPE",
	    InvalidContainerType: "INVALID_CONTAINTER_TYPE",
	    InvalidExperimant: "INVALID_EXPERIMENT",
	    InvalidLocation: "INVALID_LOCATION",
	    InvalidVendor: "INVALID_VENDOR",
	    InvalidItem: "INVALID_ITEM",
	    InvalidCas: "INVALID_CAS",
	    InvalidDetail: "INVALID_DETAIL",
	    AlreadyExisted: "ALREADY_EXISTED",
	    OperationDenied: "OPERATION_DENIED"
    }
};

export const Events = {
    CardActionClick: 'CardActionClick'
};

export interface EventListener<T> {
    tag: string;
    handler: (v: T) => T;
}

export interface EventArgs {
    canceled: boolean;
}

// Customized structure

export interface UserInformation {
    name: string,
    displayName: string,
    userGroup: string,
    permission: string,
    activeTime?: string,
    accessAddress?: string,
    disabled?: boolean,
    update?: string
}

export interface ChangeUserInformation {
    displayName?: string | boolean,
    password?: string | boolean,
    userGroup?: string | boolean,
    disabled?: boolean
}
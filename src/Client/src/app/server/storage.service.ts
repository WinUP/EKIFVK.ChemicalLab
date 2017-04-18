import { Injectable } from '@angular/core';
import { MessageService } from './message.service';
import { Messages, LocalData, StorageType } from './structure'
import { Configuration } from './configuration'

@Injectable()
export class StorageService {
    private memoryStorage = {};
    
    constructor(private message: MessageService) {
        window.addEventListener('storage', listener => {
            if (listener.storageArea == window.localStorage && listener.key == Configuration.sessionStorageRoot) {
                var oldData = JSON.parse(listener.oldValue);
                var newData = JSON.parse(listener.newValue);
                Object.keys(newData).forEach(e => {
                    if (newData[e] != oldData[e]) {
                        console.log(`----- [StorageService] UPDATE ${e} ->`);
                        console.log(newData[e]);
                        this.message.prepare().tag(Messages.Storage).value<LocalData>({
                            key: e, value: newData[e], type: StorageType.Session
                        }).go();
                    }
                });
            }
        });
    }

    public local(key: string, value: any): void {
        console.log(`----- [StorageService] LOCAL key: ${key} value ->`);
        console.log(value);
        var data = window.localStorage.getItem(Configuration.localStorageRoot);
        var storage: any = {};
        if (data != null) storage = JSON.parse(data);
        storage[key] = value;
        window.localStorage.setItem(Configuration.localStorageRoot, JSON.stringify(storage));
        this.message.prepare().tag(Messages.Storage).value<LocalData>({
            key: key, value: value, type: StorageType.Local
        }).go();
    }

    public session(key: string, value: any): void {
        console.log(`----- [StorageService] SESSION key: ${key} value ->`);
        console.log(value);
        var data = window.localStorage.getItem(Configuration.sessionStorageRoot);
        var storage: any = {};
        if (data != null) storage = JSON.parse(data);
        storage[key] = value;
        window.localStorage.setItem(Configuration.sessionStorageRoot, JSON.stringify(storage));
        this.message.prepare().tag(Messages.Storage).value<LocalData>({
            key: key, value: value, type: StorageType.Session
        }).go();
    }

    public cache(key: string, value: any): void {
        console.log(`----- [StorageService] CACHE key: ${key} value ->`);
        console.log(value);
        this.memoryStorage[key] = value;
        this.message.prepare().tag(Messages.Storage).value<LocalData>({
            key: key, value: value, type: StorageType.Cache
        }).go();
    }

    public read<T>(type: StorageType, key: string): T {
        var result: any = null;
        if (type == StorageType.Local) {
            var localData = window.localStorage.getItem(Configuration.localStorageRoot);
            if (localData == null) return result;
            var storage = JSON.parse(localData);
            result = storage[key];
        }
        else if (type == StorageType.Session) {
            var localData = window.localStorage.getItem(Configuration.sessionStorageRoot);
            if (localData == null) return result;
            var storage = JSON.parse(localData);
            result = storage[key];
        }
        else if (type == StorageType.Cache) {
            result = this.memoryStorage[key];
        }
        return result == undefined ? null : result;
    }
}
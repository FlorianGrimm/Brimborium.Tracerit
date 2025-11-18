import type { Subscribable } from "rxjs";

export interface TraceableSubject /*extends Subscribable<any>*/ {
    //_getTraceableInformation?: () => TraceableInformation;
    _TraceableInformation?: TraceableInformation;
}

export interface NamedSubject {
    name?: string;
}

export class TraceableInformation {
    constructor(name: string | undefined, identifier: number) {
        this.name = name;
        this.identifier = identifier;
    }
    public name: string | undefined;
    public identifier: number;

    toString() {
        return  `${this.name}#${this.identifier}`;
    }
}

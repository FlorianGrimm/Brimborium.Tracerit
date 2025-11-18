import { combineLatest, Observable, ObservableInput, ObservedValueOf, Observer, Subscription, tap } from "rxjs";
import { getExistingTraceableInformation, getTraceableInformation, TraceableSubjectGraphService } from "./traceable-subject-graph.service";
import { TraceableInformation, TraceableSubject } from "./TraceableSubject";
import { inject } from "@angular/core";


export function combineLatestSubject<T extends Record<string, ObservableInput<any>>>(
    args:{
        dictObservable: T,
        name: string,
        fnReport?: ((name: string, message: string, value: T | undefined) => void)
    }
): CombineLatestSubject<T> {
    return new CombineLatestSubject(args.dictObservable, args.name, args.fnReport);
}

export class CombineLatestSubject<T extends Record<string, ObservableInput<any>>>
    implements TraceableSubject {
    private readonly graph = inject(TraceableSubjectGraphService).getGraph();
    public readonly name: string;
    private readonly sourcesObject: T;
    _TraceableInformation?: TraceableInformation;

    constructor(
        dictObservable: T,
        name: string,
        private readonly fnReport?: ((name: string, message: string, value: T | undefined) => void)
    ) {
        const tiThis = getTraceableInformation(this, name);
        this.name = tiThis.toString();
        this.sourcesObject = dictObservable;
        for (const key in dictObservable) {
            if (!Object.hasOwn(dictObservable, key)) continue;
            const element = dictObservable[key];
            const tiElement = getExistingTraceableInformation(element);
            if (tiElement === undefined) { continue; }
            this.graph.addSubscription(tiElement, tiThis, key);
        }
    }

    addGraphSubscription(target$: Observer<any> | TraceableSubject, name?: string) {
        const tiTarget = getTraceableInformation(target$);
        this.graph.addSubscription(this._TraceableInformation!, tiTarget, name);
    }

    combineLatest(): Observable<{ [K in keyof T]: ObservedValueOf<T[K]>; }> {
        const result = combineLatest(this.sourcesObject)
        if (undefined !== this.fnReport) {
            return result.pipe(
                tap({
                    subscribe: () => {
                        if (undefined !== this.fnReport) {
                            this.fnReport(this.name, "subscribe", undefined);
                        }
                    },
                    next: (value) => {
                        if (undefined !== this.fnReport) {
                            this.fnReport(this.name, "next", value);
                        }
                    },
                    complete: () => {
                        if (undefined !== this.fnReport) {
                            this.fnReport(this.name, "complete", undefined);
                        }
                    },
                    error: (err) => {
                        if (undefined !== this.fnReport) {
                            this.fnReport(this.name, "error", err);
                        }
                    },
                    unsubscribe: () => {
                        if (undefined !== this.fnReport) {
                            this.fnReport(this.name, "unsubscribe", undefined);
                        }
                    }
                })
            );
        } else {
            return result;
        }
    }

    // pipeAndSubscribe<R extends Exclude<any, undefined>>(
    //     subscription: Subscription|undefined,
    //     observer: Observer<R>,
    //     pipeFn: (subject: Observable<{ [K in keyof T]: ObservedValueOf<T[K]> }>) => Observable<R | undefined>,
    //     fnNext?: (value: R, observer: Observer<R>) => void
    // ): Subscription {
    //     this.addGraphSubscription(observer);
    //     const combineLatest$ = this.combineLatest();
    //     const pipe$: Observable<R | undefined> = pipeFn(combineLatest$);
    //     const localSubscription = new Subscription();
    //     if (undefined !== subscription) { subscription.add(localSubscription); }
    //     localSubscription.add(
    //         pipe$.subscribe({
    //             next: (value) => {
    //                 if (undefined !== value) {
    //                     if (undefined !== fnNext) {
    //                         fnNext(value, observer);
    //                     } else {
    //                         observer.next(value);
    //                     }
    //                 }
    //             },
    //             complete: () => {
    //                 localSubscription.unsubscribe();
    //             },
    //             error: () => {
    //                 localSubscription.unsubscribe();
    //             }
    //         }));
    //     return localSubscription;
    // }
    // static defaultLog(name: string, message: string, value: any | undefined) {
    //     console.log(name, message, value);
    // }

}

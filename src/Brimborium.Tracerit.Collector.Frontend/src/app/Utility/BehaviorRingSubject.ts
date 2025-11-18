import { BehaviorSubject, Observable, Observer, Subject, SubjectLike, Subscribable, Subscription } from "rxjs";
import { DependecyRingSubject } from "./DependecyRingSubject";
import { MasterRingSubject } from "./MasterRingSubject";
import type { TraceableSubject } from "./TraceableSubject";
import { getTraceableInformation } from "./traceable-subject-graph.service";

export function createBehaviorRingSubject<T>(
    subscription: Subscription,
    source: (() => Observable<T>),
    initialValue: T,
    ring: number,
    name: string,
    conditionSubject?: MasterRingSubject,
    fnPaused?: (that: BehaviorRingSubject<T>, ring: undefined | MasterRingSubject) => void,
    fnReport?: ((name: string, message: string, value: T | undefined) => void)
): BehaviorRingSubject<T> {
    const result = new BehaviorRingSubject<T>(initialValue, ring, name, subscription, conditionSubject, fnPaused, fnReport);
    const observer = source();
    const localSubscription = new Subscription();
    subscription.add(localSubscription);
    localSubscription.add(
        observer.subscribe({
            next: (value) => {
                result.next(value);
            },
            complete: () => {
                localSubscription.unsubscribe();
            },
            error: () => {
                localSubscription.unsubscribe();
            }
        })
    );
    return result;
}


export class BehaviorRingSubject<T extends Exclude<any, undefined>> extends BehaviorSubject<T> implements DependecyRingSubject {
    private state = {
        enabled: true as boolean,
        currentRing: 0 as number,
        pending: false,
        value: undefined as T | undefined,
    };
    public readonly name: string;
    constructor(
        value: T,
        public readonly ring: number,
        name: string,
        public readonly subscriptionRing?: Subscription,
        public readonly ring$?: MasterRingSubject,
        private readonly fnPaused?: (that: BehaviorRingSubject<T>, ring: undefined | MasterRingSubject) => void,
        private readonly fnReport?: ((name: string, message: string, value: T | undefined) => void)
    ) {
        super(value);
        this.name = getTraceableInformation(this, name).toString();

        if (undefined !== subscriptionRing && undefined !== ring$) {
            // later ring$.graph?.addSubject(this);
            this.state.enabled = true;
            this.state.currentRing = ring$.getValue();
            subscriptionRing.add(
                ring$.subscribe({
                    next: (value) => {
                        this.setCurrentRing(value);
                    },
                    complete: () => {
                        this.setCurrentRing(-1);
                    },
                    error: () => {
                        this.setCurrentRing(-1);
                    }
                }));
        }
    }
    setCurrentRing(value: number) {
        this.state.currentRing = value;
        if (this.state.currentRing >= this.ring) {
            if (this.state.pending) {
                this.state.pending = false;

                const nextValue = this.state.value;
                this.state.value = undefined;

                if (undefined !== this.fnReport) {
                    this.fnReport(this.name, "resume", nextValue);
                }
                super.next(nextValue!);
            }
        }
    }

    public override next(value: T): void {
        if (this.state.enabled) {
            if (this.state.currentRing >= this.ring) {
                if (undefined !== this.fnReport) {
                    this.fnReport(this.name, "next", value);
                }
                super.next(value);
            } else {
                this.state.value = value;
                if (this.state.pending) {
                    if (undefined !== this.fnReport) {
                        this.fnReport(this.name, "throttle", value);
                    }
                } else {
                    this.state.pending = true;
                    if (undefined !== this.fnReport) {
                        this.fnReport(this.name, "pause", value);
                    }
                }
                if (undefined !== this.fnPaused) {
                    this.fnPaused(this, this.ring$);
                }
            }
        } else {
            if (undefined !== this.fnReport) {
                this.fnReport(this.name, "next", value);
            }
            super.next(value);
        }
    }

    validateRing(target$: BehaviorRingSubject<any>, name?: string) {
        /*
        if (this.ring < target$.ring) {
            // ok
            // later this.ring$?.graph?.addSubscription(this, target$);
        } else {
            throw new Error(`${this.name}.ring:${this.ring} >= ${target$.name}.ring:${target$.ring} ${name}`);
        }
        */
        return this;
    }

    pipeAndSubscribe<R, O extends SubjectLike<R>>(
        subscription: Subscription | undefined,
        observer: O,
        pipeFn: (subject: Observable<T>) => Observable<R>,
        fnNext?: (value: R, observer: SubjectLike<R>) => void
    ): Subscription {
        //this.validateRing(observer);
        const pipe$: Observable<R | undefined> = pipeFn(this);
        const localSubscription = new Subscription();
        if (undefined !== subscription) { subscription.add(localSubscription); }
        localSubscription.add(
            pipe$.subscribe({
                next: (value) => {
                    if (undefined !== value) {
                        if (undefined !== fnNext) {
                            fnNext(value, observer);
                        } else {
                            if (observer.next === undefined) { return; }
                            observer.next(value);
                        }
                    }
                },
                complete: () => {
                    localSubscription.unsubscribe();
                },
                error: (err) => {
                    localSubscription.unsubscribe();
                }
            }));
        return localSubscription;
    }

    static defaultLog(name: string, message: string, value: any | undefined) {
        console.log(name, message, value);
    }
}
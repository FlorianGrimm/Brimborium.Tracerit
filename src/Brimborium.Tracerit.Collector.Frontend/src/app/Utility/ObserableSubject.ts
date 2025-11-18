import { BehaviorSubject, Observable, subscribeOn, Subscription } from "rxjs";

export function createObserableSubject<T>(
    args: {
        obs: Observable<T>
        value: T,
        subscribtion: Subscription
    }
): ObserableSubject<T> {
    return new ObserableSubject<T>(args);
}
export class ObserableSubject<T> extends BehaviorSubject<T> {
    constructor(args: {
        obs: Observable<T>
        value: T,
        subscribtion: Subscription
    }) {
        super(args.value);

        args.subscribtion.add(
            args.obs.subscribe({
                next: (value) => {
                    this.next(value);
                },
                complete: () => {
                    this.complete();
                },
                error: (err) => {
                    this.error(err);
                }
            }));
    }
}
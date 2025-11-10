import { BehaviorSubject, Observable, Observer, Subscription } from "rxjs"

export function createBehaviorSubject<T>(
    subscription: Subscription,
    source: (() => Observable<T>),
    initialValue: T): BehaviorSubject<T> {
    const result = new BehaviorSubject<T>(initialValue);
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

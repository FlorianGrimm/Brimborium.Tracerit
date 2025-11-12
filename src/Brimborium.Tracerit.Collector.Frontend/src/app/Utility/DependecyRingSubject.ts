import { Observable } from "rxjs";

export interface DependecyRingSubject<T = any> {
    readonly ring: number;
    readonly name: string;

    validateRing(target$: DependecyRingSubject<any>, name?:string): void;
}

export const DependecyRingSubjectStatic = {
    InstanceIndex: 0
}

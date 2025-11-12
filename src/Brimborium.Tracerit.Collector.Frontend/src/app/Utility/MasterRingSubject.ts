import { BehaviorSubject } from "rxjs";
import { Graph } from "./master-ring.service";


export class MasterRingSubject extends BehaviorSubject<number> {
    graph: Graph | undefined = undefined;

    constructor(
        value: number,
        public readonly name: string
    ) {
        super(value);
    }
}

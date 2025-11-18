import type { TraceableInformation } from "./TraceableSubject";

export class GraphNode {
    public readonly name: string;
    constructor(
        public readonly subject: TraceableInformation
    ) {
        this.name = subject.name ?? "";
    }
    toString() {
        return this.name;
    }
}

export class GraphEdge {
    public key: string | undefined;
    constructor(
        public readonly from: TraceableInformation,
        public readonly to: TraceableInformation, 
        public readonly fromAlias: string|undefined
    ) {
        if (undefined === from.name || undefined === to.name) {
            this.key = undefined;
        } else {
            this.key = `${from.name} -> ${to.name}`;
        }
    }

    toString() {
        return this.key ?? "";
    }
}

export class Graph {
    public readonly nodes = new Map<string, GraphNode>();
    public readonly edges = new Map<string, GraphEdge>();

    constructor() {
    }

    addSubject(subject: TraceableInformation) {
        if (undefined === subject.name) { return; }

        let node = this.nodes.get(subject.name);
        if (undefined === node) {
            node = new GraphNode(subject);
            this.nodes.set(node.name, node);
        }
    }

    addSubscription(from: TraceableInformation, to: TraceableInformation, fromAlias: string|undefined) {
        if (undefined === from.name
            || undefined === to.name) { return; }
        let fromNode = this.nodes.get(from.name);
        if (undefined === fromNode) {
            fromNode = new GraphNode(from);
            this.nodes.set(from.name, fromNode);
        }
        let toNode = this.nodes.get(to.name);
        if (undefined === toNode) {
            toNode = new GraphNode(to);
            this.nodes.set(to.name, toNode);
        }
        const edge = new GraphEdge(from, to, fromAlias);
        const edgeKey = edge.toString();
        if (!this.edges.has(edgeKey)) {
            this.edges.set(edgeKey, edge);
        }
    }
}
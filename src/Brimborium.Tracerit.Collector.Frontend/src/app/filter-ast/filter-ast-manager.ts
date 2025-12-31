import { effect, inject, InputSignal, OutputEmitterRef, signal, untracked } from "@angular/core";
import { FilterAstNode, LogLineValue, PropertyHeader } from "@app/Api";
import { DataService } from "@app/Utility/data-service";
import {
    convertFilterAstNodeToUIFilterAstNode,
    convertUIFilterAstNodeToFilterAstNode,
    FilterOperatorsDisplay,
    getPathUiNodeById,
    getUiNodeById,
    getValidLogLineValue,
    initialUIFilterAstNodeSelection,
    mapFilterOperatorsDisplayByOperator,
    replaceUiNode,
    replaceUiNodeFn,
    ReplaceUiNodeFn,
    UIFilterAstNode,
    UIFilterAstNodeSelection
} from "@app/Utility/filter-ast-node";
import { Subscription } from "rxjs";

export class FilterAstManager {
    readonly dataService = inject(DataService);
    readonly $uiNodeRoot = signal<UIFilterAstNode>(convertFilterAstNodeToUIFilterAstNode(null));
    readonly $selection = signal<UIFilterAstNodeSelection>(initialUIFilterAstNodeSelection);
    rootNode: FilterAstNode | null;

    constructor(
        rootNode: FilterAstNode | null,
        subscription: Subscription | undefined,
        nodeInput: InputSignal<FilterAstNode | null> | undefined,
        readonly nodeChanged: OutputEmitterRef<FilterAstNode> | undefined
    ) {
        this.rootNode = rootNode;
        if (nodeInput != null) {
            const e = effect(() => {
                const node = nodeInput();
                this.setRootNode(node);
            });
            subscription?.add(() => e.destroy());
        }
        const uiNodeRoot = convertFilterAstNodeToUIFilterAstNode(rootNode);
        this.$uiNodeRoot.set(uiNodeRoot);
    }

    setRootNode(rootNode: FilterAstNode | null) {
        if (rootNode == null) {
            rootNode = {
                operator: "and",
                listChild: [],
                value: undefined,
            };
        }
        if (Object.is(this.rootNode, rootNode)) {
            // console.log("setRootNode is", this.rootNode, rootNode);
            return this;
        } else {
            // console.log("setRootNode is not ", this.rootNode, rootNode);
            this.rootNode = rootNode;
            const uiNodeRoot = convertFilterAstNodeToUIFilterAstNode(rootNode);
            this.$uiNodeRoot.set(uiNodeRoot);

            return this;
        }
    }

    setRootNodeUi(uiNodeRoot: UIFilterAstNode) {
        this.$uiNodeRoot.set(uiNodeRoot);
        const rootNode = convertUIFilterAstNodeToFilterAstNode(uiNodeRoot);
        this.$uiNodeRoot.set(uiNodeRoot);
        this.nodeChanged?.emit(rootNode);
        return this;
    }

    setSelection(selection: UIFilterAstNodeSelection) {
        this.$selection.set(selection);
        return this;
    }

    replaceUiNode(replaceNode: UIFilterAstNode) {
        const uiNodeRoot = this.$uiNodeRoot();
        const nextUiNodeRoot = replaceUiNode(uiNodeRoot, replaceNode);
        this.setRootNodeUi(nextUiNodeRoot);
        return this;
    }

    replaceUiNodeFn(fnReplace: ReplaceUiNodeFn): UIFilterAstNode {
        const uiNodeRoot = this.$uiNodeRoot();
        const nextUiNodeRoot = replaceUiNodeFn(uiNodeRoot, fnReplace);
        this.setRootNodeUi(nextUiNodeRoot);
        return nextUiNodeRoot;
    }

    modifyFilterAstNode(nextuiNodeSelected: UIFilterAstNode, nextSelection: UIFilterAstNodeSelection) {
        const selection = this.$selection();
        const uiNodeRoot = this.$uiNodeRoot();
        if (selection == null) { throw new Error('selection is null'); }
        if (uiNodeRoot == null) { throw new Error('uiNodeRoot is null'); }
        const nextUiNodeRoot = replaceUiNode(uiNodeRoot, nextuiNodeSelected);
        this.setRootNodeUi(nextUiNodeRoot).setSelection(nextSelection);
    }

    modifyFilterAstNodeFN(fnReplace: ReplaceUiNodeFn, nextSelection: UIFilterAstNodeSelection) {
        const selection = this.$selection();
        const uiNodeRoot = this.$uiNodeRoot();
        if (selection == null) { throw new Error('selection is null'); }
        if (uiNodeRoot == null) { throw new Error('uiNodeRoot is null'); }
        const nextUiNodeRoot = replaceUiNodeFn(uiNodeRoot, fnReplace);
        this.setRootNodeUi(nextUiNodeRoot).setSelection(nextSelection);
    }

    setPropertyName(name: string | null, node: UIFilterAstNode) {

    }
    setPropertyHeader(header: PropertyHeader | null, node: UIFilterAstNode) {
        debugger;
        if (header == null) {
            if (node.value?.name == null) { return false; }
            const value: (LogLineValue | undefined | null) = (node.value == null)
                ? undefined
                : ({ ...node.value, name: "" });
            const nextNode: UIFilterAstNode = {
                ...node,
                value: value,
            };
            this.replaceUiNode(nextNode);
            return true;
        } else {
            if (header.name == node.value?.name) {
                return false;
            }
            const typeValue = header.typeValue;
            const value = getValidLogLineValue(typeValue, node.value?.value);
            const nextNode = {
                ...node,
                value: {
                    name: header.name,
                    typeValue: typeValue,
                    value: value,
                }
            };
            this.replaceUiNode(nextNode);
            return true;
        }
    }

    appendListChild(uiNode: UIFilterAstNode) {
        const uiNodeRoot = this.$uiNodeRoot();
        if (uiNodeRoot == null) { throw new Error('uiNodeRoot is null'); }

        const appendUiNode: UIFilterAstNode = {
            id: crypto.randomUUID(),
            operator: 'eq',
            listChild: [],
            value: undefined
        };

        const nextUiNode: UIFilterAstNode = {
            ...uiNode,
            listChild: [...(uiNode.listChild ?? []), appendUiNode]
        };
        const nextUiNodeRoot = replaceUiNode(uiNodeRoot, nextUiNode);
        this.setRootNodeUi(nextUiNodeRoot).setSelection({ id: appendUiNode.id, property: 'operator' });
    }

    appendFilterAstNodeListChild(uiNode: UIFilterAstNode, appendUiNode: UIFilterAstNode) {
        const selection = this.$selection();
        const uiNodeRoot = this.$uiNodeRoot();
        if (selection == null) { throw new Error('selection is null'); }
        if (uiNodeRoot == null) { throw new Error('uiNodeRoot is null'); }

        const nextUiNode: UIFilterAstNode = {
            ...uiNode,
            listChild: [...(uiNode.listChild ?? []), appendUiNode]
        };
        const nextUiNodeRoot = replaceUiNode(uiNodeRoot, nextUiNode);
        const nextSelection: UIFilterAstNodeSelection = {
            id: appendUiNode.id,
            property: 'operator',
        };
        this.setRootNodeUi(nextUiNodeRoot).setSelection(nextSelection);
    }

    onSetOperator(nextFod: FilterOperatorsDisplay) {
        const selection = this.$selection();
        const uiNodeRoot = this.$uiNodeRoot();
        const id = selection.id;
        if (id == null) { return; }

        const uiNodeSelected = getUiNodeById(uiNodeRoot, id);
        if (uiNodeSelected == null) { return; }

        if (uiNodeSelected.operator === nextFod.operator) { return; }

        // const operator = selection.operator;
        const operator = uiNodeSelected.operator;
        const currentFod = operator == null ? undefined : mapFilterOperatorsDisplayByOperator.get(operator);
        if (selection.property == 'operator') {
            if (currentFod != null) {
                if (currentFod.kind == nextFod.kind) {
                    // use default
                } else {
                    // kind changed
                    if (nextFod.kind == 'list') {
                        // was value new list
                        if (uiNodeSelected.value === undefined) {
                            // use default
                        } else {
                            const newNode: UIFilterAstNode = {
                                id: crypto.randomUUID(),
                                operator: nextFod.operator,
                                listChild: [uiNodeSelected],
                                value: undefined,
                            };
                            const nextUiNodeRoot = replaceUiNodeFn(uiNodeRoot,
                                (node) => {
                                    if (id == node.id) {
                                        return ({ node: newNode, replaced: true });
                                    }
                                    return ({ node: node, replaced: false });
                                });
                            const nextSelection: UIFilterAstNodeSelection = {
                                id: newNode.id,
                                property: 'operator'
                            };
                            this.setRootNodeUi(nextUiNodeRoot).setSelection(nextSelection);
                            return;
                        }
                    } else if (nextFod.kind == 'value') {
                        // was list new value
                        const listChild = (uiNodeSelected.listChild ?? []);
                        if (listChild.length == 0) {
                            // no children
                            const nextuiNodeSelected: UIFilterAstNode = {
                                ...uiNodeSelected,
                                operator: nextFod.operator
                            };
                            this.modifyFilterAstNode(nextuiNodeSelected, selection);
                            return;
                        } else {
                            // has children
                            const nextUiNode: UIFilterAstNode = {
                                id: crypto.randomUUID(),
                                operator: nextFod.operator,
                                listChild: [uiNodeSelected],
                                value: undefined
                            };
                            const nextSelection: UIFilterAstNodeSelection = {
                                id: nextUiNode.id,
                                property: 'operator'
                            };
                            const nextUiNodeRoot = replaceUiNodeFn(uiNodeRoot,
                                (node) => {
                                    if (id == node.id) {
                                        return ({ node: nextUiNode, replaced: true });
                                    }
                                    return ({ node: node, replaced: false });
                                });
                            this.setRootNodeUi(nextUiNodeRoot).setSelection(nextSelection);
                        }
                        return;
                    } else {
                        return;
                    }
                }
            }

            // default set operator
            {
                const nextuiNodeSelected: UIFilterAstNode = {
                    ...uiNodeSelected,
                    operator: nextFod.operator
                };
                this.modifyFilterAstNode(nextuiNodeSelected, selection);
            }
            return;
        } else if (selection.property == 'list') {
            // append 
            {
                const appendUiNode: UIFilterAstNode = {
                    id: crypto.randomUUID(),
                    operator: nextFod.operator,
                    listChild: [],
                    value: undefined
                };
                this.appendFilterAstNodeListChild(uiNodeSelected, appendUiNode);
            }
            return;
        } else {
            return;
        }
    }
    onSetProperty(nextProperty: PropertyHeader) {
        const selection = this.$selection();
        const uiNodeRoot = this.$uiNodeRoot();
        const id = selection.id;
        if (id == null) { return; }

        const uiNodeSelected = getUiNodeById(uiNodeRoot, id);
        if (uiNodeSelected == null) { return; }

        const operator = uiNodeSelected.operator;
        const currentFod = operator == null ? undefined : mapFilterOperatorsDisplayByOperator.get(operator);
        if (currentFod == null) { return; }


        if (selection.property == 'operator') {
            if (currentFod.kind == 'value') {
                // set value
                const nextValueValue = getValidLogLineValue(nextProperty.typeValue, uiNodeSelected.value?.value)
                const nextValue: LogLineValue = {
                    name: nextProperty.name,
                    typeValue: nextProperty.typeValue,
                    value: nextValueValue! as any,
                };
                const nextuiNodeSelected: UIFilterAstNode = {
                    ...uiNodeSelected,
                    value: nextValue,
                };
                this.modifyFilterAstNode(nextuiNodeSelected, selection);
                return;
            } else if (currentFod.kind == 'list') {
                // append 
                const newValue: LogLineValue = {
                    name: nextProperty.name,
                    typeValue: nextProperty.typeValue,
                    value: undefined! as any,
                };
                const newUINodeChild: UIFilterAstNode = {
                    id: crypto.randomUUID(),
                    operator: 'eq',
                    listChild: undefined,
                    value: newValue,
                };

                const nextuiNodeSelected: UIFilterAstNode = {
                    ...uiNodeSelected,
                    listChild: [...(uiNodeSelected.listChild ?? []), newUINodeChild],
                };
                this.modifyFilterAstNode(nextuiNodeSelected, selection);
                return;
            }
        } else if (selection.property == 'list') {
            // append 
            const newValue: LogLineValue = {
                name: nextProperty.name,
                typeValue: nextProperty.typeValue,
                value: undefined! as any,
            };
            const newUINodeChild: UIFilterAstNode = {
                id: crypto.randomUUID(),
                operator: 'eq',
                listChild: undefined,
                value: newValue,
            };

            const nextuiNodeSelected: UIFilterAstNode = {
                ...uiNodeSelected,
                listChild: [...(uiNodeSelected.listChild ?? []), newUINodeChild],
            };
            this.modifyFilterAstNode(nextuiNodeSelected, selection);
            return;
        }
    }

    onRemoveNode() {
        const selection = this.$selection();
        const uiNodeRoot = this.$uiNodeRoot();
        if (uiNodeRoot == null) { return; }
        const id = selection.id;
        if (id == null) { return; }
        const listUiNodePath = getPathUiNodeById(uiNodeRoot, id);
        if (listUiNodePath == null || listUiNodePath.length == 0) { return; }

        const uiNodeSelected = listUiNodePath[0];
        if (listUiNodePath.length == 1) {
            const nextUiNode: UIFilterAstNode = {
                id: uiNodeSelected.id,
                operator: "and",
                listChild: [],
                value: undefined,
            };
            this.modifyFilterAstNode(nextUiNode, selection);
        } else {
            const parentUiNode = listUiNodePath[1];
            const listChild = (parentUiNode.listChild ??= [])
                .filter(item => item.id != uiNodeSelected.id);
            const nextParentUiNode: UIFilterAstNode = {
                ...parentUiNode,
                listChild: listChild
            };
            this.modifyFilterAstNode(nextParentUiNode, selection);
        }
    }

    onInsertParent() {
        const selection = this.$selection();
        const uiNodeRoot = this.$uiNodeRoot();
        if (uiNodeRoot == null) { return; }

        const id = selection.id;
        if (id == null) { return; }

        const uiNodeSelected = getUiNodeById(uiNodeRoot, id);
        if (uiNodeSelected == null) { return; }

        const parentUiNode: UIFilterAstNode = {
            id: crypto.randomUUID(),
            operator: 'and',
            listChild: [uiNodeSelected],
            value: undefined
        };
        const nextUiNodeRoot = replaceUiNodeFn(uiNodeRoot, (node) => {
            if (id == node.id) {
                return ({ node: parentUiNode, replaced: true });
            }
            return ({ node: node, replaced: false });
        });
        const nextSelection: UIFilterAstNodeSelection = {
            id: parentUiNode.id,
            property: 'operator',
        };
        this.setRootNodeUi(nextUiNodeRoot).setSelection(nextSelection);
    }
}

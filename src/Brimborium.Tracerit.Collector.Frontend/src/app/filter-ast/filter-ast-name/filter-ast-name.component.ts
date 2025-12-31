import {
  afterRenderEffect, Component, inject,
  input, signal,
  untracked,
  viewChild,
  viewChildren
} from '@angular/core';
import { LucideAngularModule } from 'lucide-angular';
import { emptyUIFilterAstNode } from '../../Utility/filter-ast-node';
import { DataService } from '../../Utility/data-service';
import { UIFilterAstNode } from '../../Utility/filter-ast-node';
import { PropertyHeader } from '../../Api';
import { FilterAstManager } from '../filter-ast-manager';
import { AppIconComponent } from '../../app-icon/app-icon.component';
import {
  Combobox,
  ComboboxDialog,
} from '@angular/aria/combobox';
import {
  Listbox,
  Option
} from '@angular/aria/listbox';
import { OverlayModule } from '@angular/cdk/overlay';
import { DepDataService } from '../../Utility/dep-data.service';
import { ComboboxFilterComponent } from '../../Utility/combobox-filter/combobox-filter.component';

@Component({
  selector: 'app-filter-ast-name',
  imports: [
    LucideAngularModule,
    OverlayModule,
    ComboboxFilterComponent,
  ],
  templateUrl: './filter-ast-name.component.html',
  styleUrl: './filter-ast-name.component.scss',
})
export class FilterAstNameComponent {
  readonly dataService = inject(DataService);
  readonly depDataService = inject(DepDataService);
  readonly depThis = this.depDataService.wrap(this);
  readonly appIcon = new AppIconComponent();

  readonly filterAstManager = input.required<FilterAstManager>();
  readonly uiNode = input.required<UIFilterAstNode>();

  readonly uiNodeProp = this.depThis.createProperty<UIFilterAstNode>({
    name: 'uiNode',
    initialValue: emptyUIFilterAstNode,
    input: { input: this.uiNode }
  });

  readonly listAllHeaderSortedByName = this.depThis.createProperty<readonly PropertyHeader[]>({
    name: 'listAllHeaderSortedByName',
    initialValue: []
  })
    .withSource({
      sourceDependency: { listAllHeaderSortedByName: this.dataService.listAllHeaderSortedByName.dependencyPublic(), },
      sourceTransform: ({ listAllHeaderSortedByName }) => (listAllHeaderSortedByName)
    });
  readonly $listAllHeaderSortedByName = this.listAllHeaderSortedByName.asSignal();

  readonly uiNodeValueName = this.depThis.createProperty<string>({
    name: 'uiNodeValueName',
    initialValue: '',
    compare: (a, b) => a === b,
    enableReport: true,
    sideEffect: {
      fn: (value: string) => {
        //this.listPropertyHeaderFilter.setValue(value);
        console.log('uiNodeValueName', this.uiNodeValueName.name, value);
        const header = this.listAllHeaderSortedByName.getValue().find(item => value === item.name) ?? null;
        this.filterAstManager().setPropertyHeader(header, this.uiNode());
      }
    }
  }).withSource({
    sourceDependency: { uiNodeProp: this.uiNodeProp.dependencyInner() },
    sourceTransform: ({ uiNodeProp }) => (uiNodeProp.value?.name ?? '')
  });
  readonly $uiNodeValueName = this.uiNodeValueName.asSignal();

  // readonly $listPropertyHeaderFilter = signal<string>('');
  // readonly listPropertyHeaderFilter = this.depThis.createPropertyForSignal(
  //   this.$listPropertyHeaderFilter,
  //   {
  //     name: 'listPropertyHeaderFilter',
  //     compare: (a, b) => a === b,
  //     enableReport: true,
  //   })

  // $selectedHeader = signal<PropertyHeader[]>([]);
  // selectedHeader = this.depThis.createPropertyForSignal(
  //   this.$selectedHeader,
  //   {
  //     name: 'selectedHeader',
  //     compare: (a, b) => {
  //       if (a.length === b.length) {
  //         if (1 === a.length) {
  //           return a[0].id === b[0].id;
  //         }
  //         return true;
  //       }
  //       return false
  //     },
  //     /*
  //     sideEffect: {
  //       fn: (value) => {
  //         const filterAstManager = this.filterAstManager();
  //         const uiNode = this.uiNode();
  //         if (filterAstManager == null || uiNode == null) { return; }
  //         const nextValue = (1 === value.length)
  //           ? value[0]
  //           : null;
  //         const currentName = uiNode.value?.name ?? null;
  //         const nextName = nextValue?.name ?? null;
  //         if (currentName == nextName) { return; }
  //         console.log("sideEffect",nextName);
  //         filterAstManager.setPropertyHeader(nextValue, uiNode);
  //       }
  //     }
  //     */
  //   }
  // ).withSource({
  //   sourceDependency: {
  //     listHeader: this.dataService.listAllHeaderSortedByName.dependencyPublic(),
  //     uiNodeProp: this.uiNodeProp.dependencyInner()
  //   },
  //   sourceTransform: ({ listHeader, uiNodeProp }) => {
  //     const name = uiNodeProp.value?.name;
  //     const result = listHeader.filter(i => i.name === name);
  //     return result;
  //   }
  // });

  // readonly listPropertyHeader = this.depThis.createProperty<readonly PropertyHeader[]>({
  //   name: 'listPropertyHeader',
  //   initialValue: [],
  //   enableReport: true,
  //   report: (property, message, value) => {
  //     console.log(`${property.name} ${message}`, value);
  //   },
  // }).withSource({
  //   sourceDependency: {
  //     listHeader: this.dataService.listAllHeaderSortedByName.dependencyPublic(),
  //     filter: this.listPropertyHeaderFilter.dependencyPublic(),
  //   },
  //   sourceTransform: ({ listHeader, filter }) => {
  //     if (filter === '') { return listHeader; }
  //     return listHeader.filter(item => item.name.toLowerCase().startsWith(filter.toLowerCase()));
  //     //const re = new RegExp( RegExp.escape(filter));
  //   }
  // });
  // readonly $listPropertyHeader = this.listPropertyHeader.asSignal();

  constructor() {
    this.depThis.executePropertyInitializer();
    // filterAstManager.setPropertyHeader(nextValue, uiNode);
  }

  // onNameChanged(value: string, mustMatch: boolean) {
  //   this.listPropertyHeaderFilter.setValue(value);
  //   const listPropertyHeader = this.listPropertyHeader.getValue();
  //   if (mustMatch) {
  //     if (listPropertyHeader.length === 1) {
  //       const header = listPropertyHeader[0]
  //       if (this.listPropertyHeaderFilter.getValue() !== value) {
  //         this.listPropertyHeaderFilter.setValue(header.name);
  //       }
  //       this.filterAstManager().setPropertyHeader(header, this.uiNode());
  //     } else {
  //       this.filterAstManager().setPropertyHeader(null, this.uiNode());
  //     }
  //   }
  // }
}

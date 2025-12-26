import {
  afterRenderEffect,
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
  signal,
  untracked,
  viewChild,
  viewChildren,
} from '@angular/core';
import { NgClass } from '@angular/common';
import { LucideAngularModule, Plus, Minus } from 'lucide-angular';
import { emptyUIFilterAstNode, getValidLogLineValue, mapFilterOperatorsDisplayByOperator, replaceUiNode } from '@app/Utility/filter-ast-node';
import { DataService } from '@app/Utility/data-service';
import { initialUIFilterAstNodeSelection, OutputFilterAstNode, UIFilterAstNode, UIFilterAstNodeSelection } from '@app/Utility/filter-ast-node';
import { FilterAstNodeValue } from "@app/filter-ast/filter-ast-node-value.component/filter-ast-node-value.component";
import { LogLineValue, PropertyHeader } from '@app/Api';
import { FilterAstManager } from '../filter-ast-manager';
import { AppIconComponent } from '@app/app-icon/app-icon.component';
import { CdkContextMenuTrigger } from "@angular/cdk/menu";
import {
  Combobox,
  ComboboxDialog,
  ComboboxInput,
  ComboboxPopupContainer,
} from '@angular/aria/combobox';
import { Listbox, Option } from '@angular/aria/listbox';
import { OverlayModule } from '@angular/cdk/overlay';
import { DepDataService } from '@app/Utility/dep-data.service';

@Component({
  selector: 'app-filter-ast-name',
  imports: [
    LucideAngularModule,
    OverlayModule,
    ComboboxDialog,
    Combobox,
    ComboboxInput,
    ComboboxPopupContainer,
    Listbox,
    Option
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

  dialog = viewChild(ComboboxDialog);
  /** The combobox listbox popup. */
  listbox = viewChild<Listbox<string>>(Listbox);
  /** The options available in the listbox. */
  options = viewChildren<Option<string>>(Option);
  /** A reference to the ng aria combobox. */
  combobox = viewChild<Combobox<string>>(Combobox);

  readonly uiNodeValueName = this.depThis.createProperty<string>({
    name: 'uiNodeValueName',
    initialValue: '',
    compare: (a, b) => a === b,
    enableReport: true,
    // sideEffect: {
    //   fn: (value) => {
    //     this.listPropertyHeaderFilter.setValue(value);
    //   }
    // }
  }).withSource({
    sourceDependency: { uiNodeProp: this.uiNodeProp.dependencyInner() },
    sourceTransform: ({ uiNodeProp }) => (uiNodeProp.value?.name ?? '')
  });

  // HERE
  readonly $uiNodeValueName = this.uiNodeValueName.asSignal();
  //readonly $uiNodeValueName = signal<string>('');

  readonly $listPropertyHeaderFilter = signal<string>('');
  readonly listPropertyHeaderFilter = this.depThis.createPropertyForSignal(
    this.$listPropertyHeaderFilter,
    {
      name: 'listPropertyHeaderFilter',
      compare: (a, b) => a === b,
      enableReport: true,
    })

  $selectedHeader = signal<PropertyHeader[]>([]);
  selectedHeader = this.depThis.createPropertyForSignal(
    this.$selectedHeader,
    {
      name: 'selectedHeader',
      compare: (a, b) => {
        if (a.length === b.length) {
          if (1 === a.length) {
            return a[0].id === b[0].id;
          }
          return true;
        }
        return false
      },
      /*
      sideEffect: {
        fn: (value) => {
          const filterAstManager = this.filterAstManager();
          const uiNode = this.uiNode();
          if (filterAstManager == null || uiNode == null) { return; }
          const nextValue = (1 === value.length)
            ? value[0]
            : null;
          const currentName = uiNode.value?.name ?? null;
          const nextName = nextValue?.name ?? null;
          if (currentName == nextName) { return; }
          console.log("sideEffect",nextName);
          filterAstManager.setPropertyHeader(nextValue, uiNode);
        }
      }
      */
    }
  ).withSource({
    sourceDependency: {
      listHeader: this.dataService.listAllHeaderSortedByName.dependencyPublic(),
      uiNodeProp: this.uiNodeProp.dependencyInner()
    },
    sourceTransform: ({ listHeader, uiNodeProp }) => {
      const name = uiNodeProp.value?.name;
      const result = listHeader.filter(i => i.name === name);
      return result;
    }
  });

  readonly listPropertyHeader = this.depThis.createProperty<readonly PropertyHeader[]>({
    name: 'listPropertyHeader',
    initialValue: [],
    enableReport: true,
    report: (property, message, value) => {
      console.log(`${property.name} ${message}`, value);
    },
  }).withSource({
    sourceDependency: {
      listHeader: this.dataService.listAllHeaderSortedByName.dependencyPublic(),
      filter: this.listPropertyHeaderFilter.dependencyPublic(),
    },
    sourceTransform: ({ listHeader, filter }) => {
      if (filter === '') { return listHeader; }
      return listHeader.filter(item => item.name.toLowerCase().startsWith(filter.toLowerCase()));
      //const re = new RegExp( RegExp.escape(filter));
    }
  });
  readonly $listPropertyHeader = this.listPropertyHeader.asSignal();

  constructor() {
    this.depThis.executePropertyInitializer();

    afterRenderEffect(() => {
      if (this.dialog() && this.combobox()?.expanded()) {
        untracked(() => this.listbox()?.gotoFirst());
        this.positionDialog();
      }
    });
    afterRenderEffect(() => {
        const selectedHeader = this.$selectedHeader();
        if (selectedHeader.length > 0) {
          untracked(() => {
            this.dialog()?.close();
            //this.value.set(this.$selectedHeader()[0]);
            this.listPropertyHeaderFilter.setValue('');
            const filterAstManager = this.filterAstManager();
            const uiNode = this.uiNode();
            if (filterAstManager == null || uiNode == null) { return; }
            const nextValue = (1 === selectedHeader.length)
              ? selectedHeader[0]
              : null;
            const currentName = uiNode.value?.name ?? null;
            const nextName = nextValue?.name ?? null;
            if (currentName == nextName) { return; }
            filterAstManager.setPropertyHeader(nextValue, uiNode);
          }
          );
        }
    });
    afterRenderEffect(() => this.listbox()?.scrollActiveItemIntoView());
  }

  positionDialog() {
    const dialog = this.dialog()!;
    const combobox = this.combobox()!;
    const comboboxRect = combobox.inputElement()?.getBoundingClientRect();
    const scrollY = window.scrollY;
    if (comboboxRect) {
      dialog.element.style.width = `${comboboxRect.width}px`;
      dialog.element.style.top = `${comboboxRect.bottom + scrollY + 4}px`;
      dialog.element.style.left = `${comboboxRect.left - 1}px`;
    }
  }

  onNameChanged(value: string, mustMatch: boolean) {
    this.listPropertyHeaderFilter.setValue(value);
    const listPropertyHeader = this.listPropertyHeader.getValue();
    if (mustMatch) {
      if (listPropertyHeader.length === 1) {
        const header = listPropertyHeader[0]
        if (this.listPropertyHeaderFilter.getValue() !== value) {
          this.listPropertyHeaderFilter.setValue(header.name);
        }
        this.filterAstManager().setPropertyHeader(header, this.uiNode());
      } else {
        this.filterAstManager().setPropertyHeader(null, this.uiNode());
      }
    }
  }

  onComboBoxToggle() {
    const combobox = this.combobox();
    if (combobox) {
      const expanded = combobox.expanded()
      if (expanded) {
        combobox.close();
      } else {
        combobox.open();
      }
    }

  }
}

import {
  Component,
  ComponentRef,
  EnvironmentInjector,
  Type,
  ViewContainerRef,
  createComponent,
  input,
  signal,
  effect,
  EffectRef
} from '@angular/core';
import {
  CommonModule,
  NgComponentOutlet
} from '@angular/common';
import {
  LucideAngularModule,
  SquareChevronDown,
  SquareChevronRight,
  SquareX,
  Expand,
  Shrink
} from 'lucide-angular';

const $activeWindow = signal<undefined | ToolWindow>(undefined);

@Component({
  selector: 'app-tool-window',
  imports: [
    CommonModule,
    LucideAngularModule,
    NgComponentOutlet
  ],
  templateUrl: './tool-window.html',
  styleUrl: './tool-window.scss',
})
export class ToolWindow {
  readonly title = input<string>("title");

  readonly WindowButtonExpanded = SquareChevronDown;
  readonly WindowButtonCollapsed = SquareChevronRight;
  readonly WindowButtonClose = SquareX;
  readonly WindowButtonMaximize = Expand;
  readonly WindowButtonRestore = Shrink;
  readonly contentComponent = input<Type<any> | null>(null);

  // Reference to the component itself for self-destruction
  private componentRef?: ComponentRef<ToolWindow>;

  readonly $windowTop = signal(0);
  readonly $windowLeft = signal(0);
  readonly $windowWidth = signal(200);
  readonly $windowHeight = signal(100);
  readonly $windowZIndex = signal(GlobalStateConstants.activeWindowZIndex);

  // Collapse state
  readonly $isCollapsed = signal(false) ;
  private heightBeforeCollapse = 0;

  // Drag state
  private isDragging = false;
  private dragStartX = 0;
  private dragStartY = 0;
  private windowStartX = 0;
  private windowStartY = 0;

  // Resize state
  private isResizing = false;
  private resizeDirection = '';
  private resizeStartX = 0;
  private resizeStartY = 0;
  private resizeStartWidth = 0;
  private resizeStartHeight = 0;
  private resizeStartLeft = 0;
  private resizeStartTop = 0;

  public $isActive = signal(true);

  constructor() {
    $activeWindow.set(this);
    this.$windowZIndex.set(GlobalStateConstants.activeWindowZIndex);
    this.disposeActiveWindow = effect(() => {
      const activeWindow = $activeWindow();
      const isActive = Object.is(activeWindow, this);
      this.$isActive.set(isActive);
      const nextZIndex = isActive ? GlobalStateConstants.activeWindowZIndex : GlobalStateConstants.inactiveWindowZIndex;
      this.$windowZIndex.set(nextZIndex);
    });
  }

  disposeActiveWindow: EffectRef | undefined;

  ngOnDestroy() {
    if (this.disposeActiveWindow != null) {
      this.disposeActiveWindow.destroy();
      this.disposeActiveWindow = undefined;
    }
  }

  setComponentRef(ref: ComponentRef<ToolWindow>) {
    this.componentRef = ref;
  }

  onMouseDown() {
    $activeWindow.set(this);
    this.$windowZIndex.set(GlobalStateConstants.activeWindowZIndex);
  }

  readonly $isSizeMaximized = signal(false);
  sizeRestore: {
    windowWidth: number;
    windowHeight: number;
    windowLeft: number;
    windowTop: number;
  } | undefined;
  onSizeRestore() {
    this.$isSizeMaximized.set(false);
    if (this.sizeRestore != null) {
      this.$windowWidth.set(this.sizeRestore.windowWidth);
      this.$windowHeight.set(this.sizeRestore.windowHeight);
      this.$windowLeft.set(this.sizeRestore.windowLeft);
      this.$windowTop.set(this.sizeRestore.windowTop);
      this.sizeRestore = undefined;
    }
  }
  onSizeMaximize() {
    this.$isSizeMaximized.set(true);
    this.sizeRestore={
      windowWidth: this.$windowWidth(),
      windowHeight: this.$windowHeight(),
      windowLeft: this.$windowLeft(),
      windowTop: this.$windowTop(),
    };
    this.$windowWidth.set(window.innerWidth - 8);
    this.$windowHeight.set(window.innerHeight - 8);
    this.$windowLeft.set(4);
    this.$windowTop.set(4);
    
  }

  onWindowClose() {
    if (this.componentRef) {
      this.componentRef.destroy();
      const index = globalStateWindow.listToolWindowRef.findIndex(ref => Object.is(ref, this.componentRef));
      if (index >= 0) {
        globalStateWindow.listToolWindowRef.splice(index, 1);
      }
    }
  }

  onToggleCollapse() {
    if (this.$isCollapsed()) {
      // Expanding - restore original height
      const viewportHeight = window.innerHeight;
      const newHeight = Math.min(this.heightBeforeCollapse, viewportHeight - this.$windowTop());
      this.$windowHeight.set(newHeight);
      this.$isCollapsed.set(false);
    } else {
      // Collapsing - save current height and set to 36px
      this.heightBeforeCollapse = this.$windowHeight();
      this.$windowHeight.set(GlobalStateConstants.collapsedHeight);
      this.$isCollapsed.set(true);
    }
  }

  onHeaderMouseDown(event: MouseEvent) {
    // Don't start drag if clicking on a button
    if ((event.target as HTMLElement).closest('button')) {
      return;
    }

    this.isDragging = true;
    this.dragStartX = event.clientX;
    this.dragStartY = event.clientY;

    // Store current position from signals
    this.windowStartX = this.$windowLeft();
    this.windowStartY = this.$windowTop();

    // Prevent text selection while dragging
    event.preventDefault();

    // Add global mouse event listeners
    document.addEventListener('mousemove', this.onMouseMove);
    document.addEventListener('mouseup', this.onMouseUp);
  }

  private onMouseMove = (event: MouseEvent) => {
    if (!this.isDragging) return;

    const deltaX = event.clientX - this.dragStartX;
    const deltaY = event.clientY - this.dragStartY;

    // Calculate new position
    let newLeft = this.windowStartX + deltaX;
    let newTop = this.windowStartY + deltaY;

    // Get window dimensions
    const windowWidth = this.$windowWidth();
    const windowHeight = this.$windowHeight();

    // Get viewport dimensions
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;

    // Constrain to viewport bounds
    // Keep at least 50px of the window visible on each side
    const minVisible = 4;
    // newLeft = Math.max(-windowWidth + minVisible, Math.min(newLeft, viewportWidth - minVisible));
    newLeft = Math.max(minVisible, Math.min(newLeft, viewportWidth - windowWidth - minVisible));
    newTop = Math.max(minVisible, Math.min(newTop, viewportHeight - windowHeight - minVisible));

    // Update position using signals
    this.$windowLeft.set(newLeft);
    this.$windowTop.set(newTop);
  };

  private onMouseUp = () => {
    this.isDragging = false;
    this.isResizing = false;

    // Remove global mouse event listeners
    document.removeEventListener('mousemove', this.onMouseMove);
    document.removeEventListener('mouseup', this.onMouseUp);
    document.removeEventListener('mousemove', this.onResizeMouseMove);
    document.removeEventListener('mouseup', this.onResizeMouseUp);
  };

  onResizeMouseDown(event: MouseEvent, direction: string) {
    this.isResizing = true;
    this.resizeDirection = direction;
    this.resizeStartX = event.clientX;
    this.resizeStartY = event.clientY;

    // Store current values from signals
    this.resizeStartWidth = this.$windowWidth();
    this.resizeStartHeight = this.$windowHeight();
    this.resizeStartLeft = this.$windowLeft();
    this.resizeStartTop = this.$windowTop();

    event.preventDefault();
    event.stopPropagation();

    // Add global mouse event listeners
    document.addEventListener('mousemove', this.onResizeMouseMove);
    document.addEventListener('mouseup', this.onResizeMouseUp);
  }

  private onResizeMouseMove = (event: MouseEvent) => {
    if (!this.isResizing) return;

    const deltaX = event.clientX - this.resizeStartX;
    const deltaY = event.clientY - this.resizeStartY;

    const direction = this.resizeDirection;

    // Get viewport dimensions
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;

    // Handle horizontal resizing
    if (direction.includes('e')) {
      // Limit width to not exceed viewport
      const maxWidth = viewportWidth - this.resizeStartLeft;
      const newWidth = Math.max(200, Math.min(this.resizeStartWidth + deltaX, maxWidth));
      this.$windowWidth.set(newWidth);
    } else if (direction.includes('w')) {
      const newWidth = Math.max(200, this.resizeStartWidth - deltaX);
      const newLeft = this.resizeStartLeft + deltaX;

      // Ensure left edge doesn't go negative and width is valid
      if (newWidth >= 200 && newLeft >= 0) {
        this.$windowWidth.set(newWidth);
        this.$windowLeft.set(newLeft);
      }
    }

    // Handle vertical resizing
    if (direction.includes('s')) {
      // Limit height to not exceed viewport
      const maxHeight = viewportHeight - this.resizeStartTop;
      const newHeight = Math.max(150, Math.min(this.resizeStartHeight + deltaY, maxHeight));
      this.$windowHeight.set(newHeight);
    } else if (direction.includes('n')) {
      const newHeight = Math.max(150, this.resizeStartHeight - deltaY);
      const newTop = this.resizeStartTop + deltaY;

      // Ensure top edge doesn't go negative and height is valid
      if (newHeight >= 150 && newTop >= 0) {
        this.$windowHeight.set(newHeight);
        this.$windowTop.set(newTop);
      }
    }
  };

  private onResizeMouseUp = () => {
    this.isResizing = false;

    // Remove global mouse event listeners
    document.removeEventListener('mousemove', this.onResizeMouseMove);
    document.removeEventListener('mouseup', this.onResizeMouseUp);
  };
}


// Counter for staggering window positions
const globalStateWindow: {
  windowCounter: number;
  listToolWindowRef: ComponentRef<ToolWindow>[];
} = {
  windowCounter: 0,
  listToolWindowRef: [] as ComponentRef<ToolWindow>[],
};

export const GlobalStateConstants: {
  activeWindowZIndex: number;
  inactiveWindowZIndex: number;
  collapsedHeight: number;
} = {
  activeWindowZIndex: 1000,
  inactiveWindowZIndex: 999,
  collapsedHeight: 36,
};


export function closeWindow(
  toolWindowRef: ComponentRef<ToolWindow>
) {
  toolWindowRef.instance.onWindowClose();
}

export function openToolWindow(
  viewContainerRef: ViewContainerRef,
  environmentInjector: EnvironmentInjector,
  title: string,
  contentComponent: Type<any>
): ComponentRef<ToolWindow> {
  // Create the tool window component dynamically
  const toolWindowRef = createComponent(ToolWindow, {
    environmentInjector,
    elementInjector: viewContainerRef.injector
  });

  // Set the inputs
  toolWindowRef.setInput('title', title);
  toolWindowRef.setInput('contentComponent', contentComponent);

  // Set the component reference so it can destroy itself
  toolWindowRef.instance.setComponentRef(toolWindowRef);

  // Set initial position and size with stagger effect using signals
  const offset = (globalStateWindow.windowCounter % 10) * 30; // Stagger by 30px, reset after 10 windows
  toolWindowRef.instance.$windowLeft.set(100 + offset);
  toolWindowRef.instance.$windowTop.set(100 + offset);
  toolWindowRef.instance.$windowWidth.set(400);
  toolWindowRef.instance.$windowHeight.set(300);

  // Attach the component to the view
  viewContainerRef.insert(toolWindowRef.hostView);

  globalStateWindow.windowCounter++;
  globalStateWindow.listToolWindowRef.push(toolWindowRef);

  return toolWindowRef;
}

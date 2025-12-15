import {
  LucideAngularModule,
  MessageSquareText,
  House,
  FileStack,
  ChevronLeft,
  ChevronRight,
  Funnel,
  Spotlight,
  ChartNoAxesGantt
} from 'lucide-angular';

export class AppIconComponent {
  
  readonly LogView = MessageSquareText;
  readonly DirectoryList = FileStack;
  readonly Filter = Funnel;
  readonly Highlight = Spotlight;
  readonly TraceView = ChartNoAxesGantt;
  readonly ChevronLeft = ChevronLeft;
  readonly ChevronRight = ChevronRight;
}

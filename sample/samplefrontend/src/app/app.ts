import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import {MatSidenavModule} from '@angular/material/sidenav';
import { Header } from './layout/header/header';
import { Footer } from './layout/footer/footer';
@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,RouterLink,
    MatSidenavModule,
    Header,Footer],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
}

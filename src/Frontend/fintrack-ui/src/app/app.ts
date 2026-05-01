import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly navItems = [
    { label: 'Transações', icon: 'bi-wallet2', path: '/transactions', exact: false },
    { label: 'Dashboard', icon: 'bi-speedometer2', path: '/dashboard', exact: true }
  ];
}

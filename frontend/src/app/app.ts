import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from './layout/navbar/navbar';
import { Footer } from './layout/footer/footer';
import { ToastContainer } from './core/components/toast-container/toast-container';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, Footer, ToastContainer],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {}

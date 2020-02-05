import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

  // will store username and password as an object
  model: any = {};

  constructor(private authService: AuthService) { }

  ngOnInit() {
  }

  login(){
    this.authService.login(this.model).subscribe(next => {
      console.log('Logged in Successful');
    }, error => {
      console.log('Failed to login');
    });
  }
  
  loggedIn() {
    const token = localStorage.getItem('token');

    return !!token; // !! returns either true or false, in this case if the token contains something then true else false
  }

  logout() {
    localStorage.removeItem('token');
    console.log('Logged out');
  }
}

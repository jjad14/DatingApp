import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  baseUrl = 'http://localhost:5000/api/auth/';

constructor(private http: HttpClient) { }

/* when the token comes back from the server, it comes back as an Observable,
in order to do something with the token we need to use our rxjs operators
and in order to use these operators we need to pass them through a pipe,
pipe allows us to chain our rxjs operators to our request*/
login(model: any){
  return this.http.post(this.baseUrl + 'login', model)
    .pipe(
      map((response : any) => {
        const user = response;

        if(user) { // we want to store the token locally for easy access
            localStorage.setItem('token', user.token);
        }
      })
    );
}

register(model: any) {
  return this.http.post(this.baseUrl + 'register', model);
}

}

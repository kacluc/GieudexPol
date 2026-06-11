import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserDto } from '../models/user.dto';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl = '/api/Users';
  
  constructor(private http: HttpClient) {}

  getUserByUsername(username: string): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.apiUrl}/${username}`);
  }
}

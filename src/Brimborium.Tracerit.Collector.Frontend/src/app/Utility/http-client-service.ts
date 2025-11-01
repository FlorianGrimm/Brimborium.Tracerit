import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { LogFileInformationList, LogFileInformationListSchema } from '../Api';
import { map } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class HttpClientService {
  private http = inject(HttpClient);

  public getDirectoryList() {
    return this.http.get(
      "/api/DirectoryList",
      {
        observe: 'response',
        responseType: 'text'
      }).pipe(
        map((response) => {
          const result =LogFileInformationListSchema.safeParse(response);
          if (result.success){
            return result.data;
          } else {
            return [];
          }
        })
      );
  }

  public getFile(name: string) {
    return this.http.get<LogFileInformationList>(
      `/api/File/${encodeURIComponent(name)}`);
  }
}

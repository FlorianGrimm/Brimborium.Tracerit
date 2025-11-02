import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { LogFileInformationList, parseDirectoryBrowse, DirectoryBrowseResponse } from '../Api';
import { map, Observable } from 'rxjs';

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
        responseType: 'json',
        cache: 'no-store'
      }
    ).pipe(
      map(
        (response) => {
          // const result = ResponseDirectoryBrowseSchema.safeParse(response.body);
          // return result ;
          if (200 === response.status) {
            const result = parseDirectoryBrowse(response.body);
            return result;
          } else {
            const result: DirectoryBrowseResponse = ({ mode: "error", error: `${response.status} ${response.statusText}` });
            return result;
          }
        }
      )
    );
  }

  public getFile(name: string) {
    return this.http.get<LogFileInformationList>(
      `/api/File/${encodeURIComponent(name)}`);
  }
}

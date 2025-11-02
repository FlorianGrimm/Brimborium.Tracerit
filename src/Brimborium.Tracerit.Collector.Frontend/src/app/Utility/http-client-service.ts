import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { LogFileInformationList, parseDirectoryBrowse, DirectoryBrowseResponse, parseJsonl, GetFileResponse } from '../Api';
import { map, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class HttpClientService {
  private http = inject(HttpClient);

  public getDirectoryList(): Observable<DirectoryBrowseResponse> {
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
            const result: DirectoryBrowseResponse = {
              mode: "error",
              error: `${response.status} ${response.statusText}`
            };
            return result;
          }
        }
      )
    );
  }

  public getFile(name: string): Observable<GetFileResponse> {
    return this.http.get(
      `/api/File/${encodeURIComponent(name)}`,
      {
        observe: 'response',
        responseType: 'text',
        cache: 'no-store'
      }
    ).pipe(
      map((response) => {
        if (200 === response.status) {
          const body = response.body || "";
          const data = parseJsonl(body);
          const result: GetFileResponse = {
            mode: "success",
            data: data
          };
          return result;
        }
        {
          const result: GetFileResponse = {
            mode: "error",
            error: `${response.status} ${response.statusText}`
          };
          return result;
        }
      })
    );
  }
}

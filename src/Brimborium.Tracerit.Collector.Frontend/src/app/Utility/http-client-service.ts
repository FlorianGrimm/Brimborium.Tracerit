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
      "/api/Tracerit/DirectoryList",
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
      `/api/Tracerit/File/${encodeURIComponent(name)}`,
      {
        observe: 'response',
        responseType: 'text',
        cache: 'no-store'
      }
    ).pipe(
      map((response) => {
        if (200 === response.status) {
          const body = response.body || "";
          const { listLogline:data, nextId } = parseJsonl(body, this.nextCurrentStreamId);
          this.nextCurrentStreamId = nextId;
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

  nextCurrentStreamId:number=1;

  getCurrentStream(name:string|null|undefined) : Observable<GetFileResponse> {
    return this.http.get(
      (
        ((name === null) || (name === undefined) ||(name === ""))
        ? "/api/Tracerit/CurrentStream" 
        : `/api/Tracerit/CurrentStream/${encodeURIComponent(name)}`
      ),
      {
        observe: 'response',
        responseType: 'text',
        cache: 'no-store'
      }
    ).pipe(
      map((response) => {
        if (200 === response.status) {
          const body = response.body || "";
          const { listLogline:data, nextId } = parseJsonl(body, this.nextCurrentStreamId);
          this.nextCurrentStreamId = nextId;
          const result: GetFileResponse = {
            mode: "success",
            data: data
          };
          // console.log("getCurrentStream", name, result.data.length);
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

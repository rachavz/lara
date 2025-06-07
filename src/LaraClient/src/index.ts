﻿/*
Copyright (c) 2019-2020 Integrative Software LLC
Created: 5/2019
Author: Pablo Carbonell
*/

/*
Lara is a server-side DOM rendering library for C#.
This file is the client runtime for Lara.
https://laraui.com
*/

//#region Framework

import { autocompleteStart, autocompleteStop } from "./Autocomplete"
import { block, unblock } from "./Blocker"
import { EventResult, EventResultType } from "./DeltaInterfaces"
import { clean } from "./Initializer"
import { collectValues, collectMessage } from "./InputCollector"
import { Sequencer } from "./Sequencer"
import { processResult } from "./Worker"
import { SocketEventParameters, loadFiles } from "./SocketEvents"

let documentId: string
let lastEventNumber: number
let sequencer: Sequencer

export function initialize(id: string, keepAliveInterval: number): void {
  sequencer = new Sequencer()
  documentId = id
  lastEventNumber = 0
  window.addEventListener("unload", terminate, false)
  clean(document)
  const json = document.head.getAttribute("data-lara-initialdelta")
  if (json) {
    const result = JSON.parse(json)
    processEventResult(result)
  }
  if (keepAliveInterval) {
    window.setInterval(sendKeepAlive, keepAliveInterval)
  }
}

export function getDocumentId(): string {
  return documentId
}

function terminate(): void {
  const url = "/_discard?doc=" + documentId
  navigator.sendBeacon(url)
}

function sendKeepAlive() {
  const id = getDocumentId()
  const url = "/_keepAlive?doc=" + id
  navigator.sendBeacon(url)
}

export enum PropagationType {
  // eslint-disable-next-line no-unused-vars
  Default = 0,
  // eslint-disable-next-line no-unused-vars
  StopPropagation = 1,
  // eslint-disable-next-line no-unused-vars
  StopImmediatePropagation = 2
}

export interface PlugOptions {
  EventName: string
  Block?: boolean
  BlockElementId?: string
  BlockHTML?: string
  BlockShownId?: string
  ExtraData?: string
  LongRunning?: boolean
  IgnoreSequence?: boolean
  Propagation?: PropagationType
  PreventDefault?: boolean
  UploadFiles?: boolean
}

export function plugEvent(
  el: EventTarget,
  ev: Event,
  options: PlugOptions
): void {
  stopPropagation(ev, options)
  plug(el, options)
}

function stopPropagation(ev: Event, options: PlugOptions): void {
  if (options.PreventDefault) {
    ev.preventDefault()
  }
  if (options.Propagation == PropagationType.StopImmediatePropagation) {
    ev.stopImmediatePropagation()
  } else if (options.Propagation == PropagationType.StopPropagation) {
    ev.stopPropagation()
  }
}

export function plug(el: EventTarget, options: PlugOptions): void {
  if (options.LongRunning) {
    plugWebSocket(el, options)
  } else {
    plugAjax(el, options)
  }
}

function plugWebSocket(el: EventTarget, plug: PlugOptions): void {
  block(plug)
  const promise = buildSocketParameters(el, plug)
  promise.then(
    (params) => {
      plugWebSocketStart(plug, params)
    },
    (reason) => {
      // eslint-disable-next-line no-console
      console.log(reason)
      location.reload()
    }
  )
}

function plugWebSocketStart(
  plug: PlugOptions,
  params: SocketEventParameters
): void {
  const url = getSocketUrl("/_event")
  const socket = new WebSocket(url)
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  socket.onopen = function (_event) {
    socket.onmessage = async function (e1) {
      await onSocketMessage(e1.data, params.EventNumber)
    }
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    socket.onclose = function (_e2) {
      unblock(plug)
    }
    const json = JSON.stringify(params)
    socket.send(json)
  }
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  socket.onerror = function (_event) {
    // eslint-disable-next-line no-console
    console.log("Error on websocket communication. Reloading.")
    location.reload()
  }
}

function getSocketUrl(name: string): string {
  let url: string
  if (location.protocol == "https:") {
    url = "wss://"
  } else {
    url = "ws://"
  }
  return url + window.location.host + name
}

async function buildSocketParameters(
  el: EventTarget,
  plug: PlugOptions
): Promise<SocketEventParameters> {
  const params = createSocketParameters(el, plug)
  params.SocketFiles = await loadFiles(plug)
  return params
}

function createSocketParameters(
  el: EventTarget,
  plug: PlugOptions
): SocketEventParameters {
  const params = new SocketEventParameters()
  if (plug.IgnoreSequence) {
    params.EventNumber = 0
  } else {
    params.EventNumber = getEventNumber()
  }
  params.DocumentId = documentId
  params.ElementId = getTargetId(el)
  params.EventName = plug.EventName
  params.Message = collectMessage(plug)
  return params
}

function getEventNumber(): number {
  lastEventNumber++
  return lastEventNumber
}

async function onSocketMessage(
  json: string,
  eventNumber: number
): Promise<void> {
  await sequencer.waitForTurn(eventNumber)
  const result = JSON.parse(json) as EventResult
  processEventResult(result)
}

export function getTargetId(target: EventTarget): string {
  if (target instanceof Element) {
    return target.id
  } else {
    return ""
  }
}

function plugAjax(el: EventTarget, plug: PlugOptions): void {
  block(plug)
  const eventNumber = getEventNumber()
  const url = getEventUrl(el, plug.EventName, eventNumber)
  const ajax = new XMLHttpRequest()
  ajax.onreadystatechange = async function () {
    if (this.readyState == 4) {
      await processAjax(this, eventNumber)
      unblock(plug)
    }
  }
  const data = collectValues(plug)
  ajax.open("POST", url, true)
  if (data) {
    ajax.send(data)
  } else {
    ajax.send()
  }
}

export interface MessageOptions {
  key: string
  data?: string
  block?: boolean
  blockElementId?: string
  blockHtml?: string
  blockShowElementId?: string
  longRunning?: boolean
}

export function sendMessage(options: MessageOptions): void {
  const params: PlugOptions = {
    EventName: "_" + options.key,
    Block: options.block,
    BlockElementId: options.blockHtml,
    BlockHTML: options.blockHtml,
    BlockShownId: options.blockShowElementId,
    ExtraData: options.data,
    LongRunning: options.longRunning
  }
  plug(document.head, params)
}

async function processAjax(
  ajax: XMLHttpRequest,
  eventNumber: number
): Promise<void> {
  if (ajax.status == 200) {
    await processAjaxResult(ajax, eventNumber)
  } else {
    processAjaxError(ajax)
  }
}

function getEventUrl(
  el: EventTarget,
  eventName: string,
  eventNumber: number
): string {
  return (
    "/_event?doc=" +
    documentId +
    "&el=" +
    getTargetId(el) +
    "&ev=" +
    eventName +
    "&seq=" +
    eventNumber.toString()
  )
}

async function processAjaxResult(
  ajax: XMLHttpRequest,
  eventNumber: number
): Promise<void> {
  await sequencer.waitForTurn(eventNumber)
  const result = JSON.parse(ajax.responseText) as EventResult
  processEventResult(result)
}

function processEventResult(result: EventResult): void {
  if (result.ResultType == EventResultType.Success) {
    if (result.List) {
      processResult(result.List)
    }
  } else if (result.ResultType == EventResultType.NoSession) {
    location.reload()
  }
}

function processAjaxError(ajax: XMLHttpRequest): void {
  if (ajax.responseText) {
    document.write(ajax.responseText)
  } else {
    // eslint-disable-next-line no-console
    console.log(
      "Internal Server Error on AJAX call. Detailed exception information on the client is turned off."
    )
  }
}

export function listenServerEvents(): void {
  plugWebSocket(document.head, {
    EventName: "_server_event",
    Block: false,
    ExtraData: "",
    LongRunning: true,
    IgnoreSequence: true
  })
}

//#endregion

//#region Built-in web components

export function autocompleteApply(payload: string): void {
  autocompleteStart(payload)
}

export function autocompleteDestroy(id: string): void {
  autocompleteStop(id)
}

//#endregion

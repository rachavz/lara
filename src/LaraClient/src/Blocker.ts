﻿/*
Copyright (c) 2019-2020 Integrative Software LLC
Created: 6/2019
Author: Pablo Carbonell
*/

import { PlugOptions } from "./index"
import "./blockUI.js"

export function block(plug: PlugOptions): void {
  if (plug.Block) {
    const target = resolveTarget(plug)
    const params = buildParameters(plug)
    if (target) {
      $(target).block(params)
    } else {
      $.blockUI(params)
    }
  }
}

export function unblock(plug: PlugOptions): void {
  if (plug.Block) {
    const target = resolveTarget(plug)
    if (target) {
      $(target).unblock()
    } else {
      $.unblockUI()
    }
  }
}

function buildParameters(plug: PlugOptions): JQBlockUIOptions {
  const result: JQBlockUIOptions = {}
  const shownId = plug.BlockShownId
  if (shownId) {
    setElementCSS(result)
  } else {
    setDefaultCSS(result)
  }
  if (shownId) {
    result.message = $("#" + shownId)
  } else if (plug.BlockHTML) {
    result.message = plug.BlockHTML
  } else {
    result.message = null
  }
  result.baseZ = 2000
  return result
}

function resolveTarget(plug: PlugOptions): Element {
  if (plug.BlockElementId) {
    const el = document.getElementById(plug.BlockElementId)
    if (el) {
      return el
    }
  }
  return null
}

function setElementCSS(options: JQBlockUIOptions): void {
  options.css = {
    position: "absolute",
    top: "50%",
    left: "50%",
    transform: "translate(-50%, -50%)",
    padding: "unset",
    margin: "unset",
    border: "unset",
    width: "unset",
    "text-align": "unset",
    color: "unset",
    "background-color": "unset"
  }
}

function setDefaultCSS(options: JQBlockUIOptions): void {
  options.css = {
    border: "none",
    padding: "15px",
    backgroundColor: "#000",
    "-webkit-border-radius": "10px",
    "-moz-border-radius": "10px",
    "border-radius": "10px",
    opacity: ".5",
    color: "#fff",
    fontSize: "18px",
    fontFamily: "Verdana,Arial",
    fontWeight: 200
  }
}

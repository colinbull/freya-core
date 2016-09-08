﻿namespace Freya.Core

open System
open System.Threading.Tasks

#if Hopac

open Hopac
open Hopac.Extensions

#endif

(* Integration

   Utility functionality for integrating the Freya model of computation with
   wider standards, in this case generally OWIN compatible servers and tools
   through the use of adapter functions from specification signatures to
   Freya signatures and vice versa. *)

(* Types

   Common type aliases and shorthand for working with OWIN systems, giving a
   more appropriate grammar when dealing with standards compliant software. *)

type OwinEnvironment =
    Freya.Core.Environment

type OwinAppFunc = 
    Func<OwinEnvironment, Task>

type OwinMidFunc =
    Func<OwinAppFunc, OwinAppFunc>

(* OwinAppFunc

   Functions for working with OWIN types, in this case OwinAppFunc, allowing
   the conversion of a Freya<_> function to an OwinAppFunc. *)

[<RequireQualifiedAccess>]
[<CompilationRepresentation (CompilationRepresentationFlags.ModuleSuffix)>]
module OwinAppFunc =

    [<CompiledName ("FromFreya")>]
    let inline ofFreya freya : OwinAppFunc =

        let freya =
            Freya.infer freya

        let init =
            State.create >> freya

#if Hopac

        OwinAppFunc (fun e ->
            Hopac.startAsTask (init e) :> Task)

#else

        OwinAppFunc (fun e ->
            Async.StartAsTask (init e) :> Task)

#endif

(* OwinMidFunc

   Functions for working with OWIN types, in this case OwinMidFunc, allowing
   the conversion of a Freya<_> function to an OwinMidFunc. *)

[<RequireQualifiedAccess>]
[<CompilationRepresentation (CompilationRepresentationFlags.ModuleSuffix)>]
module OwinMidFunc =

    [<CompiledName ("FromFreya")>]
    let inline ofFreya freya : OwinMidFunc =

        let freya =
            Freya.infer freya

        let init =
            State.create >> freya

#if Hopac

        OwinMidFunc (fun n ->
            OwinAppFunc (fun e ->
                Hopac.startAsTask (
                    Job.bind (fun (_, s) ->
                       Job.awaitUnitTask (n.Invoke (s.Environment))) (init e)) :> Task))

#else

        OwinMidFunc (fun n ->
            OwinAppFunc (fun e ->
                Async.StartAsTask (
                    async.Bind (init e, fun (_, s) ->
                        Async.AwaitTask (n.Invoke (s.Environment)))) :> Task))

#endif
/*
Copyright (c) 2022 Unordinal AB

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using UnityEngine;

/// <summary>
///     Manages the movement of another player's character.
/// </summary>
internal class BlockNetworkCharacter : MonoBehaviour
{
    /// <summary>
    ///     The speed to lerp the player's position.
    /// </summary>
    [SerializeField]
    [Tooltip("The speed to lerp the player's position")]
    public float moveLerpSpeed = 10f;

    /// <summary>
    ///     The speed to lerp the player's rotation.
    /// </summary>
    [SerializeField]
    [Tooltip("The speed to lerp the player's rotation")]
    public float rotateLerpSpeed = 50f;

    /// <summary>
    ///     The position to lerp to.
    /// </summary>
    public Vector3 NewPosition { get; set; }

    /// <summary>
    ///     The rotation to lerp to.
    /// </summary>
    public Vector3 NewRotation { get; set; }

    void Awake()
    {
        //Set initial values
        NewPosition = transform.position;
        NewRotation = transform.eulerAngles;
    }

    void Update()
    {
        //Move and rotate to new values
        transform.position = Vector3.Lerp(transform.position, NewPosition, Time.deltaTime * moveLerpSpeed);
        transform.eulerAngles = new Vector3(
            Mathf.LerpAngle(transform.eulerAngles.x, NewRotation.x, Time.deltaTime * rotateLerpSpeed),
            Mathf.LerpAngle(transform.eulerAngles.y, NewRotation.y, Time.deltaTime * rotateLerpSpeed),
            Mathf.LerpAngle(transform.eulerAngles.z, NewRotation.z, Time.deltaTime * rotateLerpSpeed)
        );
    }
}
    
